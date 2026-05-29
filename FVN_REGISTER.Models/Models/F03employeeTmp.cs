using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03employeeTmp
{
    public int Id { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public string? DeptCode { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? GenderCode { get; set; }

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? FirstWorkingDate { get; set; }

    public DateTime? EndWorkingDate { get; set; }

    public decimal? TongPhep { get; set; }

    public int? EmployeeNo { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Cvcode { get; set; }

    public int? LevelApprove { get; set; }
}
