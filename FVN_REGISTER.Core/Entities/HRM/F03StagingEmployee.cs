using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.HRM
{
    [Table("F03StagingEmployee")]
    public class F03StagingEmployee : IHrmStagingEntity
    {
        public int Id { get; set; }
        public string EntityKey { get; set; } = string.Empty;   // = EmployeeCode
        public HrmChangeAction Action { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? DeptCode { get; set; }
        public string? PositionCode { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? GenderCode { get; set; }
        public DateTime? FirstWorkingDate { get; set; }
        public DateTime? EndWorkingDate { get; set; }
        public decimal? TotalLeaveDays { get; set; }
    }

}
