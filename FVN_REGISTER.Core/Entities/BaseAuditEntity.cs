namespace FVN_REGISTER.Core.Entities
{
    public abstract class BaseAuditEntity : IAuditEntity
    {
        public int Id { get; set; }
        public bool? IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public string? LastModifiedSource { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
