using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class CombinedOTViewModel
    {
        public CreateOTRequestModel OTForm { get; set; } = new();
        public OTBalanceDto Balance { get; set; } = new();
        public List<F03OTLimitRule> LimitRules { get; set; } = new();
        public List<OTApprovalStep> ApprovalSteps { get; set; } = new();
        /// <summary>Danh sách nhân viên của phòng ban (để chọn nhanh).</summary>
        public List<OTEmployeeModel> DeptEmployees { get; set; } = new();
    }
}
