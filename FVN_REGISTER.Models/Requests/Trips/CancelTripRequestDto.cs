using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Trips;

public sealed class CancelTripRequestDto
{
    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
