namespace FVN_REGISTER.Contract.Requests.OT
{
    public sealed class OTApprovalCommandDto
    {
        public List<int> Ids { get; set; } = new();
        public int Level { get; set; }
        public string? Comment { get; set; }
    }
}
