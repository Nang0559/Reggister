using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03CompanyHolidays")]
public partial class F03CompanyHoliday : BaseAuditEntity
{
    [Required]
    public DateTime HolidayDate { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = string.Empty;

    public int Year { get; set; }

    // Dùng bool để dễ hiểu: True = Được tính là ngày nghỉ hưởng lương/phép
    // False = Ngày lễ nhưng không tính vào phép (hoặc tùy quy định HR)
    public bool IsPaidLeave { get; set; } = true;
}
