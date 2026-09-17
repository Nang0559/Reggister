using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class VF03employee
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string? DeptName { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? GenderCode { get; set; }

    public string? GenderName { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public DateTime? FirstWorkingDate { get; set; }

    public DateTime? EndWorkingDate { get; set; }

    public decimal TongPhep { get; set; }

    public int? EmployeeNo { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string Cvname { get; set; } = null!;

    public string Cvcode { get; set; } = null!;

    public string? DeptCode { get; set; }

    public bool IsActive { get; set; }
}
