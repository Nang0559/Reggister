using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class CreateOTRequestModel
    {
        [Required] public DateOnly OTDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required] public string OTType { get; set; } = "Normal"; // Normal, Weekend, Holiday
        [Required] public string DeptCode { get; set; } = null!;
        public string? Reason { get; set; }

        // Danh sách nhân viên OT
        [Required] public List<OTEmployeeModel> Employees { get; set; } = new();

        // Approver
        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }
        public string? Level1ApproveEmail { get; set; }
        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }
        public string? Level2ApproveEmail { get; set; }
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public string? Level3ApproveEmail { get; set; }
    }
}
