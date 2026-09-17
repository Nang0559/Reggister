using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Email
{
    public sealed class BatchIdsRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<int> Ids { get; set; } = new();
    }
}
