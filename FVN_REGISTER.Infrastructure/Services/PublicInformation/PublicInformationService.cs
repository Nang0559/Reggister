using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.PublicInformation;
using FVN_REGISTER.Contract.Dtos.PublicInformation;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.PublicInformation;

public sealed class PublicInformationService : IPublicInformationService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    public PublicInformationService(IUnitOfWork uow, IAuditService audit) { _uow = uow; _audit = audit; }

    public async Task<List<PublicInformationDto>> GetPublishedAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03PublicInformation>().Query().AsNoTracking()
            .Where(x => x.Status == "Published"
                && (!x.EffectiveFrom.HasValue || x.EffectiveFrom <= DateTime.Now)
                && (!x.EffectiveTo.HasValue || x.EffectiveTo >= DateTime.Now))
            .OrderByDescending(x => x.IsImportant)
            .ThenByDescending(x => x.PublishedAt)
            .ToListAsync(ct);
        return rows.Select(MapEntity).ToList();
    }

    public async Task<List<PublicInformationDto>> GetManageListAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03PublicInformation>().Query().AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return rows.Select(MapEntity).ToList();
    }

    public async Task<PublicInformationDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03PublicInformation>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity == null ? null : MapEntity(entity);
    }

    public async Task<ServiceResult<PublicInformationDto>> CreateAsync(SavePublicInformationRequest request, int actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        var entity = new F03PublicInformation { Type=request.Type.Trim(), Title=request.Title.Trim(), Summary=request.Summary?.Trim(), Content=request.Content, Status="Draft", EffectiveFrom=request.EffectiveFrom, EffectiveTo=request.EffectiveTo, IsImportant=request.IsImportant, AttachmentUrl=request.AttachmentUrl?.Trim(), CreatedBy=actorUserId };
        await _uow.Repository<F03PublicInformation>().AddAsync(entity, ct); await _uow.SaveChangesAsync(ct);
        await _audit.LogAction("PUBLIC_INFO_CREATED", actorUserId, $"Id={entity.Id}; Title={entity.Title}", ct:ct);
        return ServiceResult<PublicInformationDto>.Ok(MapEntity(entity));
    }

    public async Task<ServiceResult<PublicInformationDto>> UpdateAsync(int id, SavePublicInformationRequest request, int actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        var entity = await _uow.Repository<F03PublicInformation>().Query().FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(entity==null) return ServiceResult<PublicInformationDto>.Fail("Không tìm thấy nội dung công khai.");
        if(entity.Status=="Published") return ServiceResult<PublicInformationDto>.Fail("Không sửa trực tiếp nội dung đã Publish; hãy Archive và tạo phiên bản mới.");
        entity.Type=request.Type.Trim(); entity.Title=request.Title.Trim(); entity.Summary=request.Summary?.Trim(); entity.Content=request.Content; entity.EffectiveFrom=request.EffectiveFrom; entity.EffectiveTo=request.EffectiveTo; entity.IsImportant=request.IsImportant; entity.AttachmentUrl=request.AttachmentUrl?.Trim(); entity.ModifiedBy=actorUserId; entity.ModifiedAt=DateTime.Now;
        await _uow.SaveChangesAsync(ct); await _audit.LogAction("PUBLIC_INFO_UPDATED",actorUserId,$"Id={id}; Title={entity.Title}",ct:ct); return ServiceResult<PublicInformationDto>.Ok(MapEntity(entity));
    }

    public async Task<ServiceResult> PublishAsync(int id,int actorUserId,CancellationToken ct=default)
    { var e=await _uow.Repository<F03PublicInformation>().Query().FirstOrDefaultAsync(x=>x.Id==id,ct); if(e==null)return ServiceResult.Fail("Không tìm thấy nội dung."); if(e.Status=="Archived")return ServiceResult.Fail("Nội dung đã Archive."); e.Status="Published";e.PublishedBy=actorUserId;e.PublishedAt=DateTime.Now;e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);await _audit.LogAction("PUBLIC_INFO_PUBLISHED",actorUserId,$"Id={id}; Title={e.Title}",ct:ct);return ServiceResult.Ok(); }
    public async Task<ServiceResult> ArchiveAsync(int id,int actorUserId,CancellationToken ct=default)
    { var e=await _uow.Repository<F03PublicInformation>().Query().FirstOrDefaultAsync(x=>x.Id==id,ct); if(e==null)return ServiceResult.Fail("Không tìm thấy nội dung."); e.Status="Archived";e.ModifiedBy=actorUserId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);await _audit.LogAction("PUBLIC_INFO_ARCHIVED",actorUserId,$"Id={id}; Title={e.Title}",ct:ct);return ServiceResult.Ok(); }

    private static void Validate(SavePublicInformationRequest x){if(string.IsNullOrWhiteSpace(x.Title))throw new ArgumentException("Tiêu đề là bắt buộc.");if(x.EffectiveFrom.HasValue&&x.EffectiveTo.HasValue&&x.EffectiveTo<x.EffectiveFrom)throw new ArgumentException("Thời điểm kết thúc phải sau thời điểm bắt đầu.");if(!new[]{"Announcement","Regulation","Guide","Policy"}.Contains(x.Type,StringComparer.OrdinalIgnoreCase))throw new ArgumentException("Loại thông tin không hợp lệ.");}
    private static PublicInformationDto Map(F03PublicInformation x)=>MapEntity(x);
    private static PublicInformationDto MapEntity(F03PublicInformation x)=>new(){Id=x.Id,Type=x.Type,Title=x.Title,Summary=x.Summary,Content=x.Content,Status=x.Status,EffectiveFrom=x.EffectiveFrom,EffectiveTo=x.EffectiveTo,IsImportant=x.IsImportant,AttachmentUrl=x.AttachmentUrl,PublishedAt=x.PublishedAt};
}
