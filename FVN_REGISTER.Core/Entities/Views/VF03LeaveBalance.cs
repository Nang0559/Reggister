using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class VF03LeaveBalance
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string? EmployeeName { get; set; }
    public string? DeptCode { get; set; }
    public string? DeptName { get; set; }
    public int? GenderCode { get; set; }
    public string? GenderName { get; set; }
    public int WorkYear { get; set; }

    public decimal TotalEntitledLeave { get; set; }     // TongPhep
    public decimal TotalDaysOff { get; set; }            // TongSoNgayNghi (tổng số ngày nghỉ, có tính + không tính phép)
    public decimal LeaveDaysUsed { get; set; }            // SoNgayNghiPhep (số ngày đã dùng có tính phép)
    public decimal? RemainingLeave { get; set; }          // PhepTon

    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
}
