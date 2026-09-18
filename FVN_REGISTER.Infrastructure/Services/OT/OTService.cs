

using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace FVN_REGISTER.Infrastructure.Services.OTs
{
    public class OTService
        : BaseRequestCommandService<OTRequestUpsertDto, F03OTRequest, OTRequestSubject>,
          IOTService
    {
        private readonly IOTValidator _validator;
        private readonly IAuthorizationService _authorization;
        protected override RequestModule ModuleKind => RequestModule.Overtime;

        public OTService(
            IUnitOfWork uow,
            IApprovalWorkflowOrchestrator<OTRequestSubject> workflow,
            IOTValidator validator,
            IAuthorizationService authorization,
            ILogger<BaseRequestCommandService<OTRequestUpsertDto, F03OTRequest, OTRequestSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, workflow, logger, options)
        {
            _validator = validator;
            _authorization = authorization;
        }

        public override async Task<ServiceResult<int>> CreateAsync(OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var valResult = await _validator.ValidateCreateAsync(model, user, ct);
                if (!valResult.IsSuccess) return ServiceResult<int>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");
                if (model.Employees == null || model.Employees.Count == 0) return ServiceResult<int>.Fail("Phải chọn ít nhất 1 nhân viên.");

                var totalHours = model.Employees.Sum(e => e.OTHours);
                await using var tx = await Uow.BeginTransactionAsync(ct);
                var entity = new F03OTRequest
                {
                    EmployeeCode = user.EmployeeCode ?? "",
                    DeptCode = model.DeptCode,
                    RequestStatus = ApprovalStatus.Draft,
                    OTDate = model.OTDate,
                    OTTypeCode = model.OTTypeCode,
                    OTReasonSummary = model.OTReasonSummary,
                    TotalOTHours = totalHours,
                    CreatedBy = user.UserId,
                    CreatedAt = DateTime.Now
                };

                await Uow.Repository<F03OTRequest>().AddAsync(entity, ct);
                await Uow.SaveChangesAsync(ct);
                var empRepo = Uow.Repository<F03OTEmployee>();
                foreach (var e in model.Employees)
                {
                    await empRepo.AddAsync(new F03OTEmployee
                    {
                        OTRequestId = entity.Id,
                        EmployeeCode = e.EmployeeCode,
                        OTHours = e.OTHours,
                        OTReasonDetail = e.OTReasonDetail,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.UserId
                    }, ct);
                }
                await Uow.SaveChangesAsync(ct);

                var context = ApprovalBuildContext.ForOT(
                    employeeCode: user.EmployeeCode ?? "",
                    deptCode: entity.DeptCode ?? "",
                    positionCode: user.PositionCode ?? "",
                    totalOTHours: totalHours,
                    otTypeCode: model.OTTypeCode);
                try { await Workflow.InitApprovalAsync(entity.Id, context, ct); }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[{Component}] InitApproval failed, rolling back CreateId={Id}", ComponentName, entity.Id);
                    await tx.RollbackAsync(ct);
                    return ServiceResult<int>.Fail("Không thể khởi tạo luồng duyệt cho đơn.");
                }

                await tx.CommitAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Created OTId={Id} By={User}", ComponentName, entity.Id, user.EmployeeCode);
                return ServiceResult<int>.Ok(entity.Id, "Đã tạo đơn tăng ca thành công.");
            }
            catch (Exception ex) { return InternalError<int>(ex, "Lỗi hệ thống khi tạo đơn tăng ca."); }
        }

        public async Task<ServiceResult> JoinAsync(int otRequestId, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var repo = Uow.Repository<F03OTRequest>();
                var entity = await repo.GetByIdAsync(otRequestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
                if (entity.RequestStatus != ApprovalStatus.Pending && entity.RequestStatus != ApprovalStatus.Draft)
                    return ServiceResult.Fail("Đơn đã xử lý, không thể tham gia thêm.");

                var empRepo = Uow.Repository<F03OTEmployee>();
                var exists = await empRepo.Query().AnyAsync(x => x.OTRequestId == otRequestId && x.EmployeeCode == user.EmployeeCode && x.IsActive == true, ct);
                if (exists) return ServiceResult.Fail("Bạn đã có trong đơn này.");
                await empRepo.AddAsync(new F03OTEmployee
                {
                    OTRequestId = otRequestId,
                    EmployeeCode = user.EmployeeCode ?? "",
                    OTHours = 0,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.UserId
                }, ct);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] {User} joined OT Id={Id}", ComponentName, user.EmployeeCode, otRequestId);
                return ServiceResult.Ok("Đã tham gia đơn tăng ca.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi tham gia đơn tăng ca."); }
        }

        public async Task<ServiceResult> RemoveEmployeeAsync(int otRequestId, string employeeCode, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                await using var tx = await Uow.BeginTransactionAsync(ct);
                var entity = await Uow.Repository<F03OTRequest>().GetByIdAsync(otRequestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
                var empRepo = Uow.Repository<F03OTEmployee>();
                var row = await empRepo.Query().FirstOrDefaultAsync(x => x.OTRequestId == otRequestId && x.EmployeeCode == employeeCode && x.IsActive == true, ct);
                if (row == null) return ServiceResult.Fail("Không tìm thấy nhân viên trong đơn.");
                bool isSelf = employeeCode == user.EmployeeCode;
                bool isCreator = entity.EmployeeCode == user.EmployeeCode;
                if (!isSelf && !isCreator && !IsAdmin(user)) return ServiceResult.Fail("Không có quyền xóa nhân viên này khỏi đơn.");
                row.IsActive = false;
                entity.TotalOTHours -= row.OTHours;
                await Uow.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return ServiceResult.Ok("Đã xóa nhân viên khỏi đơn.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi xóa nhân viên khỏi đơn."); }
        }

        public async Task<ServiceResult> UpdateEmployeeOTInfoAsync(int otRequestId, List<OTEmployeeDto> updates, UserIdentityDto user, CancellationToken ct = default)
        {
            if (updates == null || updates.Count == 0) return ServiceResult.Fail("Không có dữ liệu cập nhật.");
            try
            {
                var entity = await Uow.Repository<F03OTRequest>().GetByIdAsync(otRequestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
                if (IsFinalized(entity) && !IsAdmin(user)) return ServiceResult.Fail("Đơn đã xử lý xong, không thể cập nhật.");
                var empRepo = Uow.Repository<F03OTEmployee>();
                var codes = updates.Select(u => u.EmployeeCode).ToList();
                var rows = await empRepo.Query().Where(x => x.OTRequestId == otRequestId && codes.Contains(x.EmployeeCode) && x.IsActive == true).ToListAsync(ct);
                foreach (var u in updates)
                {
                    var row = rows.FirstOrDefault(x => x.EmployeeCode == u.EmployeeCode);
                    if (row == null) continue;
                    row.OTHours = u.OTHours;
                    row.OTReasonDetail = u.OTReasonDetail;
                }
                entity.TotalOTHours = rows.Sum(x => x.OTHours);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Updated OT info for Id={Id}", ComponentName, otRequestId);
                return ServiceResult.Ok("Đã cập nhật thông tin OT.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi cập nhật thông tin OT."); }
        }

        public override async Task<ServiceResult> CancelAsync(int requestId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var entity = await Uow.Repository<F03OTRequest>().GetByIdAsync(requestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
                bool isOwner = entity.EmployeeCode == user.EmployeeCode;
                bool isAdmin = IsAdmin(user);
                if (!isOwner && !isAdmin) return ServiceResult.Fail("Không có quyền hủy đơn này.");
                var histories = await Uow.Repository<F03ApprovalHistory>().Query().Where(h => h.RequestId == requestId && h.RequestType == ModuleKind).ToListAsync(ct);
                bool anyDecided = histories.Any(h => h.Decision != DecisionType.Pending);
                if (anyDecided && !isAdmin) return ServiceResult.Fail("Đơn đã có cấp duyệt xử lý, không thể tự hủy. Liên hệ Admin.");
                ApplyCancel(entity, reason, user);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Cancelled Id={Id} By={User}", ComponentName, requestId, user.EmployeeCode);
                return ServiceResult.Ok("Đã hủy đơn tăng ca.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi hủy đơn tăng ca."); }
        }

        protected override async Task<string?> ValidateApprovalScopeAsync(
            List<int> ids, UserIdentityDto user, CancellationToken ct)
        {
            if (user.UserId <= 0)
                return "Phiên đăng nhập không hợp lệ.";

            var distinctIds = ids.Distinct().ToList();
            var entities = await Uow.Repository<F03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => distinctIds.Contains(x.Id) && x.IsActive == true)
                .Select(x => new { x.Id, x.EmployeeCode, x.DeptCode })
                .ToListAsync(ct);

            if (entities.Count != distinctIds.Count)
                return "Một hoặc nhiều đơn OT không tồn tại hoặc đã ngừng hoạt động.";

            foreach (var entity in entities)
            {
                if (!await _authorization.CanAccessAsync(
                    user,
                    SecurityFunctionCodes.OTApprove,
                    entity.EmployeeCode,
                    entity.DeptCode,
                    ct))
                {
                    return $"Không có quyền duyệt đơn OT [{entity.Id}] theo phạm vi dữ liệu được cấp.";
                }
            }

            return null;
        }

        protected override void ApplyCancel(F03OTRequest entity, string reason, UserIdentityDto user)
        {
            base.ApplyCancel(entity, reason, user);
            entity.OTReasonSummary = $"[Cancelled by {user.EmployeeCode}] {reason}";
            entity.ModifiedBy = user.UserId;
            entity.ModifiedAt = DateTime.Now;
        }

        public async Task<ServiceResult> AddEmployeesAsync(int otRequestId, List<OTEmployeeDto> newEmployees, UserIdentityDto user, CancellationToken ct = default)
        {
            if (newEmployees == null || newEmployees.Count == 0) return ServiceResult.Fail("Không có nhân viên nào để thêm.");
            try
            {
                var entity = await Uow.Repository<F03OTRequest>().GetByIdAsync(otRequestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
                bool isCreator = entity.EmployeeCode == user.EmployeeCode;
                if (!isCreator && !IsAdmin(user)) return ServiceResult.Fail("Không có quyền thêm nhân viên vào đơn này.");
                if (IsFinalized(entity) || (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.Pending))
                    return ServiceResult.Fail("Đơn đã xử lý, không thể thêm nhân viên.");
                var empRepo = Uow.Repository<F03OTEmployee>();
                var existingCodes = await empRepo.Query().Where(x => x.OTRequestId == otRequestId && x.IsActive == true).Select(x => x.EmployeeCode).ToListAsync(ct);
                foreach (var e in newEmployees.Where(e => !existingCodes.Contains(e.EmployeeCode)))
                {
                    await empRepo.AddAsync(new F03OTEmployee
                    {
                        OTRequestId = otRequestId,
                        EmployeeCode = e.EmployeeCode,
                        OTHours = e.OTHours,
                        OTReasonDetail = e.OTReasonDetail,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.UserId
                    }, ct);
                }
                entity.TotalOTHours = (await empRepo.Query().Where(x => x.OTRequestId == otRequestId && x.IsActive == true).SumAsync(x => (decimal?)x.OTHours, ct) ?? 0) + newEmployees.Sum(e => e.OTHours);
                await Uow.SaveChangesAsync(ct);
                return ServiceResult.Ok("Đã thêm nhân viên vào đơn.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi thêm nhân viên vào đơn."); }
        }
    }
}
