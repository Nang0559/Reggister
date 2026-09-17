using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Interfaces
{
    public interface IValidatableRow
    {
        RowStatus Status { get; set; }
        string? Message { get; set; }
    
    }
}
