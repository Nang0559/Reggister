

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApproverTreeLevelGroupDto
    {
        public int Level { get; set; }
        public string? RoleName { get; set; }
        public string LevelDisplayName { get; set; } = string.Empty;
        public List<ApproverTreeTypeGroupDto> Types { get; set; } = new();
    }
}
