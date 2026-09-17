using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Contract.Requests.OT;




namespace FVN_REGISTER.Contract.Dtos
{
    public class OTCombinedDataDto
    {
        public OTRequestUpsertDto OTForm { get; set; } = new();

        /// <summary>Danh sách nhân viên cùng phòng ban — load vào dialog</summary>
        public List<OTEmployeeDto> DeptEmployees { get; set; } = new();

        /// <summary>
        /// Các bước duyệt áp dụng cho đơn này, tính sẵn theo CvCode + DeptCode người tạo.
        /// Lv3(GM) Required tùy totalOTHours/otTypeCode tại thời điểm gọi —
        /// nếu user đổi giờ OT trên form, gọi lại endpoint preview để cập nhật.
        /// Dùng chung DTO với Leave (ApprovalStepCalculatedDto) — không còn entity OT riêng.
        /// </summary>
        public List<ApprovalStepSnapshotDto> ApprovalSteps { get; set; } = new();

        /// <summary>Số dư giờ OT của người tạo đơn</summary>
        public OTBalanceDto? Balance { get; set; }

        /// <summary>Rules giới hạn giờ (để client hiển thị)</summary>
        public List<OTLimitRuleDto> LimitRules { get; set; } = new();

        /// <summary>5 đơn OT gần nhất của người tạo</summary>
        public List<OTSummaryDto> RecentOTRequests { get; set; } = new();

        /// <summary>Đơn OT đã có trong ngày (để user có thể tự thêm vào)</summary>
        public List<OTSummaryDto> TodayOTRequests { get; set; } = new();

        public List<OTReasonCodeDto> OTReasonCategories { get; set; } = new();
    }
}
