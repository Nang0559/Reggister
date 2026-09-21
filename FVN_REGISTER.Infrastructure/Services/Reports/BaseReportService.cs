using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Repositories;

using FVN_REGISTER.Infrastructure.Services.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Reports
{
    /// <summary>
    /// Base chung cho mọi ReportService (OT, Leave, ...).
    /// Chứa:
    ///   - Helpers lookup Dept/Employee/OTType không lặp lại
    ///   - Helper đọc ApprovalSteps từ F03ApprovalSteps (thay Level3/5ApproveName cũ)
    ///   - BuildColumns / BuildSummary khung chuẩn để subclass điền vào
    ///   - GetLookupDepartmentsAsync / SearchLookupEmployeesAsync dùng chung
    ///   - ExportExcelAsync placeholder
    /// Domain-specific: subclass implement GetReportAsync + CanHandle + các private method riêng.
    /// </summary>
    public abstract class BaseReportService<TService>
    : BaseService<TService>, IReportService
    where TService : class
    {
        protected readonly IUnitOfWork _uow;

        protected BaseReportService(
            IUnitOfWork uow,
            ILogger<TService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        // ── IReportService contract ───────────────────────────────────────
        public abstract bool CanHandle(ReportType type);

        public abstract Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default);

        // ════════════════════════════════════════════════════════════════
        // SHARED HELPERS — dùng tự do trong subclass (không đổi)
        // ════════════════════════════════════════════════════════════════

        protected async Task<Dictionary<string, string>> LoadDeptMapAsync(
        IEnumerable<string?> deptCodes, CancellationToken ct)
        {
            var codes = deptCodes.Where(x => x != null).Select(x => x!).Distinct().ToList();
            if (codes.Count == 0) return new();

            return await _uow.Repository<F03Department>().Query()
                .AsNoTracking()
                .Where(d => codes.Contains(d.DeptCode))
                .ToDictionaryAsync(d => d.DeptCode, d => d.DeptName, ct);
        }

        protected async Task<Dictionary<string, string>> LoadEmpNameMapAsync(
        IEnumerable<string> empCodes, CancellationToken ct)
        {
            var codes = empCodes.Distinct().ToList();
            if (codes.Count == 0) return new();

            return await _uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => codes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, e => e.EmployeeName, ct);
        }
        protected async Task<Dictionary<string, string>> LoadOTTypeMapAsync(CancellationToken ct)
        {
            return await _uow.Repository<F03OTType>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive==true)
                .ToDictionaryAsync(x => x.OTTypeCode, x => x.OTTypeName, ct);
        }

        protected async Task<Dictionary<int, List<ApprovalStepDto>>> LoadApprovalStepsMapAsync(
         RequestModule requestType,
         IEnumerable<int> requestIds,
         CancellationToken ct)
        {
            var ids = requestIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var snapshots = await _uow.Repository<F03ApprovalSnapshot>().Query()
                .AsNoTracking()
                .Include(s => s.Steps)
                .Where(s => s.RequestType == requestType && ids.Contains(s.RequestId))
                .ToListAsync(ct);

            var histories = await _uow.Repository<F03ApprovalHistory>().Query()
                .AsNoTracking()
                .Where(h => h.RequestType == requestType && ids.Contains(h.RequestId))
                .ToListAsync(ct);

            var result = new Dictionary<int, List<ApprovalStepDto>>();
            foreach (var snap in snapshots)
            {
                var histForThis = histories.Where(h => h.RequestId == snap.RequestId).ToList();
                result[snap.RequestId] = ApprovalStepMapper.MapToList(snap.Steps, histForThis);
            }
            return result;
        }

        protected static string? GetApproverName(
            Dictionary<int, List<ApprovalStepDto>> stepsMap,
            int requestId,
            int level)
        {
            if (!stepsMap.TryGetValue(requestId, out var steps)) return null;
            return steps.FirstOrDefault(s => s.Level == level)?.ApproverName;
        }

        protected static IQueryable<T> ApplyAccessFilter<T>(
            IQueryable<T> query,
            UserIdentityDto user,
            string? queryDeptCode,
            Func<T, string?> getDeptCode)
        {
            return query; // placeholder — giữ nguyên như bạn đã ghi chú, không dùng trực tiếp
        }

        /// <summary>EF-compatible: lọc DeptCode trực tiếp trên IQueryable.</summary>
        // ★ SỬA: user.IsAdmin()/IsSuperAdmin() → user.Permission.IsAdmin()
        // (IsAdmin() extension đã tự bao gồm SuperAdmin, không cần OR thêm IsSuperAdmin())
        protected async Task<string> GetEffectiveReportScopeAsync(UserIdentityDto user, CancellationToken ct, params int[] functionCodes)
        {
            var scopes = await (
                from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
                join rf in _uow.Repository<F03RoleFunction>().Query().AsNoTracking() on ur.IdRole equals rf.IdRole
                join f in _uow.Repository<F03Function>().Query().AsNoTracking() on rf.IdFunction equals f.Id
                where ur.IdUser == user.UserId
                   && functionCodes.Contains(f.FunctionCode)
                   && f.IsActive == true
                select f.ScopeCode
            ).ToListAsync(ct);

            return AuthorizationScopePolicy.ResolveEffectiveScope(scopes);
        }

        // ════════════════════════════════════════════════════════════════
        // LOOKUP ENDPOINTS — dùng chung cho mọi domain report
        // ════════════════════════════════════════════════════════════════

        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(
        UserIdentityDto user, CancellationToken ct = default)
        {
            var scope = await GetEffectiveReportScopeAsync(
                user, ct,
                SecurityFunctionCodes.LeaveView,
                SecurityFunctionCodes.OTView,
                SecurityFunctionCodes.TripView,
                SecurityFunctionCodes.EquipmentView,
                SecurityFunctionCodes.AttendanceView);

            var q = _uow.Repository<F03Department>().Query().AsNoTracking().WhereActiveDept();

            if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase)
                || string.Equals(scope, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)
                || string.Equals(scope, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase))
                q = q.WhereDeptCode(user.DeptCode);

            var depts = await q
                .OrderBy(d => d.DeptName)
                .Select(d => new { d.DeptCode, d.DeptName })
                .ToListAsync(ct);

            return ServiceResult<List<KeyValuePair<string, string>>>.Ok(
                depts.Select(d => new KeyValuePair<string, string>(d.DeptCode, d.DeptName)).ToList());
        }

        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(
        string filterText, UserIdentityDto user,
        string? deptCode = null, CancellationToken ct = default)
        {
            var scope = await GetEffectiveReportScopeAsync(
                user, ct,
                SecurityFunctionCodes.LeaveView,
                SecurityFunctionCodes.OTView,
                SecurityFunctionCodes.TripView,
                SecurityFunctionCodes.EquipmentView,
                SecurityFunctionCodes.AttendanceView);

            var q = _uow.Repository<F03Employee>().Query().AsNoTracking().Where(e => e.IsActive == true);

            if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
                q = q.Where(e => e.DeptCode == user.DeptCode);
            else if (string.Equals(scope, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)
                  || string.Equals(scope, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase))
                q = q.Where(e => e.EmployeeCode == user.EmployeeCode);
            else if (!string.IsNullOrWhiteSpace(deptCode))
                q = q.Where(e => e.DeptCode == deptCode);

            if (!string.IsNullOrEmpty(filterText))
                q = q.Where(e => e.EmployeeCode.Contains(filterText)
                               || e.EmployeeName.Contains(filterText));

            var emps = await q
                .OrderBy(e => e.EmployeeCode)
                .Take(20)
                .Select(e => new { e.EmployeeCode, e.EmployeeName })
                .ToListAsync(ct);

            return ServiceResult<List<KeyValuePair<string, string>>>.Ok(
                emps.Select(e => new KeyValuePair<string, string>(
                    e.EmployeeCode, $"{e.EmployeeCode} - {e.EmployeeName}")).ToList());
        }

        public virtual Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default)
            => Task.FromResult(ServiceResult<byte[]>.Fail("Chức năng xuất Excel đang phát triển."));
    }
}
