using ClosedXML.Excel;
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Companies;
public class CompanyHolidayManagementService:BaseService<CompanyHolidayManagementService>,ICompanyHolidayManagementService
{
 private readonly IUnitOfWork _uow;
 public CompanyHolidayManagementService(IUnitOfWork uow,ILogger<CompanyHolidayManagementService> logger,IOptionsMonitor<AuthDebugOptions> options):base(logger,options)=>_uow=uow;
 public async Task<List<CompanyHolidayDto>> GetAllAsync(CancellationToken ct=default)=>await _uow.Repository<F03CompanyHoliday>().Query().AsNoTracking().OrderByDescending(x=>x.HolidayDate).Select(x=>ToDto(x)).ToListAsync(ct);
 public async Task<CompanyHolidayDto?> GetByIdAsync(int id,CancellationToken ct=default){var e=await _uow.Repository<F03CompanyHoliday>().GetByIdAsync(id,ct);return e==null?null:ToDto(e);}
 public async Task<List<int>> GetWorkYearsAsync(CancellationToken ct=default)=>await _uow.Repository<F03WorkYear>().Query().AsNoTracking().Where(x=>x.IsActive==true).Select(x=>x.WorkYear).Distinct().OrderByDescending(x=>x).ToListAsync(ct);
 public async Task<ServiceResult<CompanyHolidayDto>> CreateAsync(CompanyHolidayDto m,int userId,CancellationToken ct=default)
 { try{var repo=_uow.Repository<F03CompanyHoliday>();if(await repo.Query().AnyAsync(x=>x.HolidayDate.Date==m.HolidayDate.Date,ct))return ServiceResult<CompanyHolidayDto>.Fail("Ngày nghỉ này đã tồn tại.");if(string.IsNullOrWhiteSpace(m.Description))return ServiceResult<CompanyHolidayDto>.Fail("Mô tả ngày nghỉ không được trống.");var e=new F03CompanyHoliday{HolidayDate=m.HolidayDate.Date,Description=m.Description.Trim(),Year=m.HolidayDate.Year,TinhPhep=m.IsPaidLeave,CreatedBy=userId,CreatedAt=DateTime.Now};await repo.AddAsync(e,ct);await _uow.SaveChangesAsync(ct);return ServiceResult<CompanyHolidayDto>.Ok(ToDto(e));}catch(Exception ex){Logger.LogError(ex,"[HOLIDAY] Create error");return ServiceResult<CompanyHolidayDto>.Fail("Lỗi hệ thống khi tạo ngày nghỉ.");}}
 public async Task<ServiceResult<CompanyHolidayDto>> UpdateAsync(CompanyHolidayDto m,int userId,CancellationToken ct=default)
 { try{var repo=_uow.Repository<F03CompanyHoliday>();var e=await repo.GetByIdAsync(m.Id,ct);if(e==null)return ServiceResult<CompanyHolidayDto>.Fail("Không tìm thấy thông tin.");if(await repo.Query().AnyAsync(x=>x.Id!=m.Id&&x.HolidayDate.Date==m.HolidayDate.Date,ct))return ServiceResult<CompanyHolidayDto>.Fail("Ngày nghỉ này đã tồn tại.");e.HolidayDate=m.HolidayDate.Date;e.Description=m.Description.Trim();e.Year=m.HolidayDate.Year;e.TinhPhep=m.IsPaidLeave;e.ModifiedBy=userId;e.ModifiedAt=DateTime.Now;await _uow.SaveChangesAsync(ct);return ServiceResult<CompanyHolidayDto>.Ok(ToDto(e));}catch(Exception ex){Logger.LogError(ex,"[HOLIDAY] Update error: {Id}",m.Id);return ServiceResult<CompanyHolidayDto>.Fail("Lỗi cập nhật.");}}
 public async Task<ServiceResult> DeleteAsync(int id,CancellationToken ct=default){var repo=_uow.Repository<F03CompanyHoliday>();var e=await repo.GetByIdAsync(id,ct);if(e==null)return ServiceResult.Fail("Không tìm thấy.");repo.Remove(e);await _uow.SaveChangesAsync(ct);return ServiceResult.Ok();}
 public async Task<ServiceResult> CreateSundaysAsync(int year,int userId,CancellationToken ct=default){var repo=_uow.Repository<F03CompanyHoliday>();var date=new DateTime(year,1,1);while(date.DayOfWeek!=DayOfWeek.Sunday)date=date.AddDays(1);var n=0;while(date.Year==year){if(!await repo.Query().AnyAsync(x=>x.HolidayDate.Date==date.Date,ct)){await repo.AddAsync(new F03CompanyHoliday{HolidayDate=date,Description="Chủ nhật",Year=year,TinhPhep=false,CreatedBy=userId,CreatedAt=DateTime.Now},ct);n++;}date=date.AddDays(7);}if(n>0){await _uow.SaveChangesAsync(ct);return ServiceResult.Ok($"Đã thêm {n} ngày chủ nhật.");}return ServiceResult.Fail("Dữ liệu chủ nhật đã đầy đủ.");}
 public async Task<ServiceResult<string>> ImportExcelAsync(Stream content,string fileName,int userId,CancellationToken ct=default)
 { try{using var wb=new XLWorkbook(content);var ws=wb.Worksheets.FirstOrDefault();if(ws==null)return ServiceResult<string>.Fail("File Excel không có worksheet.");var repo=_uow.Repository<F03CompanyHoliday>();var added=0;var skipped=0;var row=2;while(!ws.Cell(row,1).IsEmpty()){ct.ThrowIfCancellationRequested();var date=ws.Cell(row,1).GetValue<DateTime?>();if(!date.HasValue)return ServiceResult<string>.Fail($"Dòng {row}: cột Ngày không hợp lệ.");var desc=ws.Cell(row,2).GetString().Trim();if(string.IsNullOrWhiteSpace(desc))return ServiceResult<string>.Fail($"Dòng {row}: thiếu Mô tả.");var paid=ws.Cell(row,3).IsEmpty()?true:ParseBool(ws.Cell(row,3).GetString());if(await repo.Query().AnyAsync(x=>x.HolidayDate.Date==date.Value.Date,ct)){skipped++;row++;continue;}await repo.AddAsync(new F03CompanyHoliday{HolidayDate=date.Value.Date,Description=desc,Year=date.Value.Year,TinhPhep=paid,CreatedBy=userId,CreatedAt=DateTime.Now},ct);added++;row++;}if(added>0)await _uow.SaveChangesAsync(ct);return ServiceResult<string>.Ok($"Import {fileName}: thêm {added}, bỏ qua {skipped} ngày đã tồn tại.");}catch(Exception ex){Logger.LogError(ex,"[HOLIDAY] Import Excel error");return ServiceResult<string>.Fail("Không đọc được file Excel. Mẫu: Cột A=Ngày, B=Mô tả, C=Tính phép.");}}
 private static bool ParseBool(string v)=>v.Trim().ToLowerInvariant() switch{"1" or "true" or "yes" or "y" or "x" or "có"=>true,"0" or "false" or "no" or "n" or "không"=>false,_=>true};
 private static CompanyHolidayDto ToDto(F03CompanyHoliday e)=>new(){Id=e.Id,HolidayDate=e.HolidayDate,Description=e.Description,Year=e.Year,IsPaidLeave=e.TinhPhep};
}