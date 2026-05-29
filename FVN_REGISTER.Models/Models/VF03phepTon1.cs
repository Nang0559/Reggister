using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03phepTon1
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string? EmployeeName { get; set; }

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public int? GenderCode { get; set; }

    public string? GenderName { get; set; }

    public int WorkYear { get; set; }

    public decimal TongPhep { get; set; }

    public decimal TongSoNgayNghi { get; set; }

    public decimal SoNgayNghiPhep { get; set; }

    public decimal? PhepTon { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
