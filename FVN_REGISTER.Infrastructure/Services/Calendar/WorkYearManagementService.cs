using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class WorkYearManagementService : IWorkYearManagementService
{
    private readonly IUnitOfWork _uow;
    public WorkYearManagementService(IUnitOfWork uow) => _uow = uow;

    public Task<List<WorkYearDto>> GetAllAsync(CancellationToken ct = default) =>
        _uow.Repository<F03WorkYear>().Query().AsNoTracking().OrderByDescending(x => x.WorkYear)
            .Select(x => new WorkYearDto { Id=x.Id, Year=x.WorkYear, YearName=$"Năm {x.WorkYear}", StartDate=x.StartDate, EndDate=x.EndDate, IsActive=x.IsActive==true, Remark=x.Remark }).ToListAsync(ct);

    public async Task<ServiceResult<WorkYearDto>> CreateAsync(WorkYearDto model, int userId, CancellationToken ct = default)
    {
        if (model.Year < 2000 || model.Year > 2100) return ServiceResult<WorkYearDto>.Fail("Năm làm việc không hợp lệ.");
        if (model.EndDate.Date < model.StartDate.Date) return ServiceResult<WorkYearDto>.Fail("Ngày kết thúc phải >= ngày bắt đầu.");
        var repo=_uow.Repository<F03WorkYear>();
        if (await repo.Query().AnyAsync(x=>x.WorkYear==model.Year,ct)) return ServiceResult<WorkYearDto>.Fail($"Năm {model.Year} đã tồn tại.");
        if (model.IsActive) await repo.Query().Where(x=>x.IsActive==true).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false),ct);
        var e=new F03WorkYear { WorkYear=model.Year, StartDate=model.StartDate.Date, EndDate=model.EndDate.Date, Remark=model.Remark?.Trim(), IsActive=model.IsActive, CreatedBy=userId, CreatedAt=DateTime.Now };
        await repo.AddAsync(e,ct); await _uow.SaveChangesAsync(ct); return ServiceResult<WorkYearDto>.Ok(ToDto(e));
    }

    public async Task<ServiceResult<WorkYearDto>> UpdateAsync(WorkYearDto model, int userId, CancellationToken ct = default)
    {
        var repo=_uow.Repository<F03WorkYear>(); var e=await repo.GetByIdAsync(model.Id,ct);
        if(e==null) return ServiceResult<WorkYearDto>.Fail("Không tìm thấy năm làm việc.");
        if(model.EndDate.Date<model.StartDate.Date) return ServiceResult<WorkYearDto>.Fail("Ngày kết thúc phải >= ngày bắt đầu.");
        if(await repo.Query().AnyAsync(x=>x.Id!=model.Id&&x.WorkYear==model.Year,ct)) return ServiceResult<WorkYearDto>.Fail($"Năm {model.Year} đã tồn tại.");
        if(model.IsActive) await repo.Query().Where(x=>x.Id!=model.Id&&x.IsActive==true).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false),ct);
        e.WorkYear=model.Year; e.StartDate=model.StartDate.Date; e.EndDate=model.EndDate.Date; e.Remark=model.Remark?.Trim(); e.IsActive=model.IsActive; e.ModifiedBy=userId; e.ModifiedAt=DateTime.Now;
        await _uow.SaveChangesAsync(ct); return ServiceResult<WorkYearDto>.Ok(ToDto(e));
    }

    public async Task<ServiceResult> SetActiveAsync(int id,bool active,int userId,CancellationToken ct=default)
    {
        var repo=_uow.Repository<F03WorkYear>(); var e=await repo.GetByIdAsync(id,ct);
        if(e==null) return ServiceResult.Fail("Không tìm thấy năm làm việc.");
        if(active) await repo.Query().Where(x=>x.Id!=id&&x.IsActive==true).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false),ct);
        e.IsActive=active; e.ModifiedBy=userId; e.ModifiedAt=DateTime.Now; await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok(active?"Đã mở khóa/kích hoạt năm làm việc.":"Đã khóa năm làm việc.");
    }

    private static WorkYearDto ToDto(F03WorkYear x)=>new(){Id=x.Id,Year=x.WorkYear,YearName=$"Năm {x.WorkYear}",StartDate=x.StartDate,EndDate=x.EndDate,IsActive=x.IsActive==true,Remark=x.Remark};
}