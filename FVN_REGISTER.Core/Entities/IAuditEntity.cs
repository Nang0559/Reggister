

namespace FVN_REGISTER.Core.Entities
{
    public interface IAuditEntity
    {
        int Id { get; }
        bool? IsActive { get; set; }
        int CreatedBy { get; }
        DateTime CreatedAt { get; }
        int? ModifiedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
    }
}
