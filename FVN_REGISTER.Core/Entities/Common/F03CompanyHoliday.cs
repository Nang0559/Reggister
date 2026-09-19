using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03CompanyHoliday")]
public partial class F03CompanyHoliday : BaseAuditEntity
{
    [Required]
    public DateTime HolidayDate { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = string.Empty;

    public int Year { get; set; }

    /// <summary>
    /// 1 = ngày nghỉ được tính vào chế độ phép theo cấu hình HR.
    /// 0 = ngày nghỉ nhưng không tính phép.
    /// </summary>
    public bool TinhPhep { get; set; }
}
