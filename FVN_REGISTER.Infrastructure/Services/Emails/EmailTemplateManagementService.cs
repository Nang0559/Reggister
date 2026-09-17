using FVN_REGISTER.Application.Interfaces.EmailTemplates;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;




namespace FVN_REGISTER.Infrastructure.Services.Emails
{
    public class EmailTemplateManagementService
        : BaseService<EmailTemplateManagementService>, IEmailTemplateManagementService
    {
        private readonly IUnitOfWork _uow;

        public EmailTemplateManagementService(
            IUnitOfWork uow,
            ILogger<EmailTemplateManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<List<EmailTemplateDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _uow.Repository<F03EmailTemplate>().Query()
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new EmailTemplateDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Subject = x.Subject,
                    Body = x.Body,
                    IsActive = x.IsActive==true,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<EmailTemplateDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var x = await _uow.Repository<F03EmailTemplate>().GetByIdAsync(id, ct);
            if (x == null) return null;

            return new EmailTemplateDto
            {
                Id = x.Id,
                Code = x.Code,
                Subject = x.Subject,
                Body = x.Body,
                IsActive = x.IsActive==true,
                CreatedAt = x.CreatedAt
            };
        }

        public async Task<ServiceResult> SaveAsync(
            EmailTemplateDto dto, int userId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Code))
                    return ServiceResult.Fail("Mã template không được để trống.");
                if (string.IsNullOrWhiteSpace(dto.Subject))
                    return ServiceResult.Fail("Tiêu đề không được để trống.");

                var repo = _uow.Repository<F03EmailTemplate>();

                if (dto.Id == 0)
                {
                    var exists = await repo.Query()
                        .AnyAsync(x => x.Code == dto.Code, ct);
                    if (exists)
                        return ServiceResult.Fail($"Mã template '{dto.Code}' đã tồn tại.");

                    var entity = new F03EmailTemplate
                    {
                        Code = dto.Code.Trim().ToUpper(),
                        Subject = dto.Subject,
                        Body = dto.Body,
                        IsActive = dto.IsActive,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    await repo.AddAsync(entity, ct);
                }
                else
                {
                    var entity = await repo.GetByIdAsync(dto.Id, ct);
                    if (entity == null)
                        return ServiceResult.Fail("Không tìm thấy template.");

                    entity.Subject = dto.Subject;
                    entity.Body = dto.Body;
                    entity.IsActive = dto.IsActive;
                    entity.ModifiedBy = userId;
                    entity.ModifiedAt = DateTime.Now;
                }

                await _uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[EMAIL_TPL] Saved: {Code}", dto.Code);
                return ServiceResult.Ok("Lưu template thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL_TPL] Save error");
                return ServiceResult.Fail("Lỗi hệ thống khi lưu template.");
            }
        }

        public async Task<ServiceResult> ToggleActiveAsync(
            int id, int userId, CancellationToken ct = default)
        {
            try
            {
                var repo = _uow.Repository<F03EmailTemplate>();
                var entity = await repo.GetByIdAsync(id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy template.");

                entity.IsActive = !entity.IsActive;
                entity.ModifiedBy = userId;
                entity.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                var status = entity.IsActive==true ? "kích hoạt" : "vô hiệu hóa";
                Logger.LogInfoIf(Debug, "[EMAIL_TPL] Toggle {Id} → {Status}", id, status);
                return ServiceResult.Ok($"Đã {status} template.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL_TPL] Toggle error");
                return ServiceResult.Fail("Lỗi hệ thống.");
            }
        }
    }
}
