

namespace FVN_REGISTER.Core.Entities
{
    public interface IStagingData
    {
        int Id {  get; }
        bool IsProcessed { get; set; }
        string? ErrorMessage { get; set; }
        DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
