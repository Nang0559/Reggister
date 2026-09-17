namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public sealed class LeaveApprovalCommandDto
    {
        public List<int> Ids { get; set; } = new();
        public int Level { get; set; }
        public string? Comment { get; set; }
    }
}
