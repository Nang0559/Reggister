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
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = "Normal"; // Normal | Weekend | Holiday
        public string DeptCode { get; set; } = string.Empty;
        public string? Reason { get; set; }

        // ===== 4 CẤP KÝ THEO QĐ-HC-03 =====
        // Cấp 1: BCH Công đoàn (bắt buộc)
        public string? UnionRepCode { get; set; }
        public string? UnionRepName { get; set; }
        public string? UnionRepEmail { get; set; }

        // Cấp 2: Ast.Chief / Chief (bắt buộc)
        public string? ChiefCode { get; set; }
        public string? ChiefName { get; set; }
        public string? ChiefEmail { get; set; }

        // Cấp 3: A.MG / MG (bắt buộc)
        public string? MGCode { get; set; }
        public string? MGName { get; set; }
        public string? MGEmail { get; set; }

        // Cấp 4: GM — bắt buộc nếu OT ngày nghỉ/lễ/Tết
        //         hoặc người OT là Ast.Chief trở lên
        public string? GMCode { get; set; }
        public string? GMName { get; set; }
        public string? GMEmail { get; set; }

        public bool RequiresGM =>
            OTType is "Weekend" or "Holiday" ||
            Employees.Any(e => e.IsAstChiefOrAbove);

        public List<OTEmployeeModel> Employees { get; set; } = new();
    }
}
