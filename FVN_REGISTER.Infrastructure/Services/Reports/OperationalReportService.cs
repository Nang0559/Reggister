using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Reports;

public sealed class OperationalReportService : BaseReportService<OperationalReportService>
{
    public OperationalReportService(IUnitOfWork uow, ILogger<OperationalReportService> logger,
        IOptionsMonitor<AuthDebugOptions> options) : base(uow, logger, options) { }

    public override bool CanHandle(ReportType type) => type is
        ReportType.TripSummaryByDept or ReportType.TripSummaryByEmployee or
        ReportType.TripDetail or ReportType.TripApprovalStatus or
        ReportType.EquipmentSummaryByDept or ReportType.EquipmentAssetDetail or
        ReportType.EquipmentRepairSummary or
        ReportType.AttendanceSummary or ReportType.AttendanceDetail;

    public override async Task<ServiceResult<ReportResultDto>> GetReportAsync(
        ReportQueryDto q, UserIdentityDto user, CancellationToken ct = default)
    {
        return q.Type switch
        {
            ReportType.TripSummaryByDept => await TripByDept(q,user,ct),
            ReportType.TripSummaryByEmployee => await TripByEmployee(q,user,ct),
            ReportType.TripDetail => await TripDetail(q,user,ct),
            ReportType.TripApprovalStatus => await TripApproval(q,user,ct),
            ReportType.EquipmentSummaryByDept => await EquipmentByDept(q,user,ct),
            ReportType.EquipmentAssetDetail => await EquipmentDetail(q,user,ct),
            ReportType.EquipmentRepairSummary => await EquipmentRepair(q,user,ct),
            ReportType.AttendanceSummary => await AttendanceByDept(q,user,ct),
            ReportType.AttendanceDetail => await AttendanceDetail(q,user,ct),
            _ => ServiceResult<ReportResultDto>.Fail("Loại báo cáo không hợp lệ")
        };
    }

    private static (DateTime from, DateTime to) Range(ReportQueryDto q)
        => (q.FromDate?.Date ?? DateTime.Today.AddMonths(-1).Date,
            q.ToDate?.Date ?? DateTime.Today.Date);

    private static bool Manager(UserIdentityDto u) => u.Permission.IsAdmin() || u.Permission.IsApprover() || u.LevelApprove > 0;

    private IQueryable<F03TripRequest> Trips(ReportQueryDto q, UserIdentityDto u)
    {
        var (from,to)=Range(q);
        var x=_uow.Repository<F03TripRequest>().Query().AsNoTracking()
            .Where(x=>x.IsActive == true && x.StartDate<=to && x.EndDate>=from);
        if (u.Permission.IsAdmin())
        {
            if (!string.IsNullOrWhiteSpace(q.DeptCode)) x=x.Where(a=>a.DeptCode==q.DeptCode);
            if (!string.IsNullOrWhiteSpace(q.EmployeeCode)) x=x.Where(a=>a.EmployeeCode==q.EmployeeCode);
        }
        else if (Manager(u))
        {
            x=x.Where(a=>a.DeptCode==u.DeptCode);
            if (!string.IsNullOrWhiteSpace(q.EmployeeCode)) x=x.Where(a=>a.EmployeeCode==q.EmployeeCode);
        }
        else x=x.Where(a=>a.EmployeeCode==u.EmployeeCode);
        return x;
    }

    private async Task<ServiceResult<ReportResultDto>> TripByDept(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Trips(q,u).GroupBy(x=>x.DeptCode).Select(g=>new { DeptCode=g.Key, Requests=g.Count(), Employees=g.Select(x=>x.EmployeeCode).Distinct().Count(), Approved=g.Count(x=>x.RequestStatus==ApprovalStatus.Approved), Estimated=g.Sum(x=>x.EstimatedCost??0) }).OrderBy(x=>x.DeptCode).ToListAsync(ct);
        return Table(ReportType.TripSummaryByDept,"Tổng hợp công tác theo phòng ban",data.Select(x=>new Dictionary<string,object?>{{"DeptCode",x.DeptCode},{"Employees",x.Employees},{"Requests",x.Requests},{"Approved",x.Approved},{"EstimatedCost",x.Estimated}}).ToList(),
            new[]{("DeptCode","Mã phòng","text"),("Employees","Số NV","number"),("Requests","Số chuyến","number"),("Approved","Đã duyệt","number"),("EstimatedCost","Chi phí dự kiến","decimal")});
    }

    private async Task<ServiceResult<ReportResultDto>> TripByEmployee(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Trips(q,u).GroupBy(x=>new{x.EmployeeCode,x.DeptCode}).Select(g=>new{g.Key.EmployeeCode,g.Key.DeptCode,Requests=g.Count(),Approved=g.Count(x=>x.RequestStatus==ApprovalStatus.Approved),Days=g.Sum(x=>(x.EndDate.Date-x.StartDate.Date).Days+1),Cost=g.Sum(x=>x.EstimatedCost??0)}).OrderBy(x=>x.EmployeeCode).ToListAsync(ct);
        return Table(ReportType.TripSummaryByEmployee,"Tổng hợp công tác theo nhân viên",data.Select(x=>new Dictionary<string,object?>{{"EmployeeCode",x.EmployeeCode},{"DeptCode",x.DeptCode},{"Requests",x.Requests},{"Approved",x.Approved},{"Days",x.Days},{"EstimatedCost",x.Cost}}).ToList(),
            new[]{("EmployeeCode","Mã NV","text"),("DeptCode","Mã phòng","text"),("Requests","Số chuyến","number"),("Approved","Đã duyệt","number"),("Days","Ngày công tác","number"),("EstimatedCost","Chi phí dự kiến","decimal")});
    }

    private async Task<ServiceResult<ReportResultDto>> TripDetail(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Trips(q,u).OrderByDescending(x=>x.StartDate).Skip(Math.Max(0,q.PageNumber-1)*Math.Clamp(q.PageSize,1,500)).Take(Math.Clamp(q.PageSize,1,500)).ToListAsync(ct);
        return Table(ReportType.TripDetail,"Chi tiết đăng ký công tác",data.Select(x=>new Dictionary<string,object?>{{"TripCode",x.TripCode},{"EmployeeCode",x.EmployeeCode},{"DeptCode",x.DeptCode},{"StartDate",x.StartDate},{"EndDate",x.EndDate},{"Destination",x.Destination},{"Purpose",x.Purpose},{"EstimatedCost",x.EstimatedCost},{"Status",x.RequestStatus.ToString()}}).ToList(),
            new[]{("TripCode","Mã chuyến","text"),("EmployeeCode","Mã NV","text"),("DeptCode","Phòng","text"),("StartDate","Từ ngày","date"),("EndDate","Đến ngày","date"),("Destination","Địa điểm","text"),("Purpose","Mục đích","text"),("EstimatedCost","Chi phí","decimal"),("Status","Trạng thái","text")});
    }

    private async Task<ServiceResult<ReportResultDto>> TripApproval(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Trips(q,u).GroupBy(x=>x.RequestStatus).Select(g=>new{Status=g.Key,Count=g.Count()}).ToListAsync(ct);
        return Table(ReportType.TripApprovalStatus,"Trạng thái phê duyệt công tác",data.Select(x=>new Dictionary<string,object?>{{"Status",x.Status.ToString()},{"Count",x.Count}}).ToList(),
            new[]{("Status","Trạng thái","text"),("Count","Số đơn","number")});
    }

    private IQueryable<F03EquipmentAsset> Equipment(ReportQueryDto q, UserIdentityDto u)
    {
        var x=_uow.Repository<F03EquipmentAsset>().Query().AsNoTracking().Where(x=>x.IsActive == true);
        if(u.Permission.IsAdmin()){if(!string.IsNullOrWhiteSpace(q.DeptCode))x=x.Where(a=>a.DeptCode==q.DeptCode);}
        else x=x.Where(a=>a.DeptCode==u.DeptCode);
        return x;
    }

    private async Task<ServiceResult<ReportResultDto>> EquipmentByDept(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Equipment(q,u).GroupBy(x=>x.DeptCode).Select(g=>new{DeptCode=g.Key,Assets=g.Count(),Value=g.Sum(x=>x.PurchasePrice),Qr=g.Count(x=>x.IsQrActive)}).OrderBy(x=>x.DeptCode).ToListAsync(ct);
        return Table(ReportType.EquipmentSummaryByDept,"Tổng hợp thiết bị theo phòng ban",data.Select(x=>new Dictionary<string,object?>{{"DeptCode",x.DeptCode},{"Assets",x.Assets},{"QrActive",x.Qr},{"PurchaseValue",x.Value}}).ToList(),
            new[]{("DeptCode","Mã phòng","text"),("Assets","Số thiết bị","number"),("QrActive","QR hoạt động","number"),("PurchaseValue","Nguyên giá","decimal")});
    }

    private async Task<ServiceResult<ReportResultDto>> EquipmentDetail(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Equipment(q,u).OrderBy(x=>x.DeptCode).ThenBy(x=>x.EquipmentCode).Skip(Math.Max(0,q.PageNumber-1)*Math.Clamp(q.PageSize,1,500)).Take(Math.Clamp(q.PageSize,1,500)).ToListAsync(ct);
        return Table(ReportType.EquipmentAssetDetail,"Chi tiết tài sản thiết bị",data.Select(x=>new Dictionary<string,object?>{{"EquipmentCode",x.EquipmentCode},{"EquipmentName",x.EquipmentName},{"DeptCode",x.DeptCode},{"SerialNumber",x.SerialNumber},{"AssetCode",x.AssetCode},{"PurchaseDate",x.PurchaseDate},{"PurchasePrice",x.PurchasePrice},{"Location",x.Location},{"QrActive",x.IsQrActive}}).ToList(),
            new[]{("EquipmentCode","Mã thiết bị","text"),("EquipmentName","Tên thiết bị","text"),("DeptCode","Phòng","text"),("SerialNumber","Serial","text"),("AssetCode","Mã tài sản","text"),("PurchaseDate","Ngày mua","date"),("PurchasePrice","Nguyên giá","decimal"),("Location","Vị trí","text"),("QrActive","QR","text")});
    }

    private async Task<ServiceResult<ReportResultDto>> EquipmentRepair(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var (from,to)=Range(q);
        var x=_uow.Repository<F03EquipmentRepairHistory>().Query().AsNoTracking().Where(x=>x.RepairDate>=from&&x.RepairDate<to.AddDays(1));
        if(!u.Permission.IsAdmin()) x=x.Where(r=>r.Asset.DeptCode==u.DeptCode);
        else if(!string.IsNullOrWhiteSpace(q.DeptCode)) x=x.Where(r=>r.Asset.DeptCode==q.DeptCode);
        var data=await x.GroupBy(r=>r.Asset.DeptCode).Select(g=>new{DeptCode=g.Key,Repairs=g.Count(),Cost=g.Sum(r=>r.RepairCost??0),Approved=g.Count(r=>r.IsApproved == true)}).OrderBy(x=>x.DeptCode).ToListAsync(ct);
        return Table(ReportType.EquipmentRepairSummary,"Tổng hợp sửa chữa thiết bị",data.Select(x=>new Dictionary<string,object?>{{"DeptCode",x.DeptCode},{"Repairs",x.Repairs},{"Approved",x.Approved},{"RepairCost",x.Cost}}).ToList(),
            new[]{("DeptCode","Mã phòng","text"),("Repairs","Số lần sửa","number"),("Approved","Đã duyệt","number"),("RepairCost","Chi phí sửa chữa","decimal")});
    }

    private IQueryable<F03AttendanceStaging> Attendance(ReportQueryDto q, UserIdentityDto u)
    {
        var (from,to)=Range(q);
        var x=_uow.Repository<F03AttendanceStaging>().Query().AsNoTracking().Where(x=>x.WorkDate>=from&&x.WorkDate<to.AddDays(1));
        if(u.Permission.IsAdmin()){if(!string.IsNullOrWhiteSpace(q.DeptCode))x=x.Where(a=>a.DeptCode==q.DeptCode);if(!string.IsNullOrWhiteSpace(q.EmployeeCode))x=x.Where(a=>a.EmployeeCode==q.EmployeeCode);}
        else if(Manager(u)){x=x.Where(a=>a.DeptCode==u.DeptCode);if(!string.IsNullOrWhiteSpace(q.EmployeeCode))x=x.Where(a=>a.EmployeeCode==q.EmployeeCode);}
        else x=x.Where(a=>a.EmployeeCode==u.EmployeeCode);
        return x;
    }

    private async Task<ServiceResult<ReportResultDto>> AttendanceByDept(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Attendance(q,u).GroupBy(x=>x.DeptCode).Select(g=>new{DeptCode=g.Key,Employees=g.Select(x=>x.EmployeeCode).Distinct().Count(),Days=g.Count(),Hours=g.Sum(x=>x.TotalHours??0),Ot=g.Sum(x=>x.OtHours??0)}).OrderBy(x=>x.DeptCode).ToListAsync(ct);
        return Table(ReportType.AttendanceSummary,"Tổng hợp chấm công theo phòng ban",data.Select(x=>new Dictionary<string,object?>{{"DeptCode",x.DeptCode},{"Employees",x.Employees},{"WorkDays",x.Days},{"TotalHours",x.Hours},{"OTHours",x.Ot}}).ToList(),
            new[]{("DeptCode","Mã phòng","text"),("Employees","Số NV","number"),("WorkDays","Số dòng công","number"),("TotalHours","Tổng giờ","decimal"),("OTHours","Giờ OT","decimal")});
    }

    private async Task<ServiceResult<ReportResultDto>> AttendanceDetail(ReportQueryDto q, UserIdentityDto u, CancellationToken ct)
    {
        var data=await Attendance(q,u).OrderByDescending(x=>x.WorkDate).ThenBy(x=>x.EmployeeCode).Skip(Math.Max(0,q.PageNumber-1)*Math.Clamp(q.PageSize,1,500)).Take(Math.Clamp(q.PageSize,1,500)).ToListAsync(ct);
        return Table(ReportType.AttendanceDetail,"Chi tiết chấm công",data.Select(x=>new Dictionary<string,object?>{{"WorkDate",x.WorkDate},{"EmployeeCode",x.EmployeeCode},{"FullName",x.FullName},{"DeptCode",x.DeptCode},{"ShiftName",x.ShiftName},{"CheckIn",x.CheckInDateTime},{"CheckOut",x.CheckOutDateTime},{"TotalHours",x.TotalHours},{"OTHours",x.OtHours},{"IsHoliday",x.IsHoliday}}).ToList(),
            new[]{("WorkDate","Ngày","date"),("EmployeeCode","Mã NV","text"),("FullName","Họ tên","text"),("DeptCode","Phòng","text"),("ShiftName","Ca","text"),("CheckIn","Vào","date"),("CheckOut","Ra","date"),("TotalHours","Tổng giờ","decimal"),("OTHours","Giờ OT","decimal"),("IsHoliday","Ngày lễ","text")});
    }

    private static ServiceResult<ReportResultDto> Table(ReportType type,string title,IReadOnlyList<Dictionary<string,object?>> rows,IReadOnlyList<(string field,string header,string dataType)> cols)
    {
        return ServiceResult<ReportResultDto>.Ok(new ReportResultDto{
            Type=type,Title=title,TotalRows=rows.Count,Rows=rows.ToList(),
            Columns=cols.Select(c=>new ReportColumnDef{Field=c.field,Header=c.header,DataType=c.dataType}).ToList(),
            Summary=new Dictionary<string,object?>{{"TotalRows",rows.Count}}
        });
    }
}