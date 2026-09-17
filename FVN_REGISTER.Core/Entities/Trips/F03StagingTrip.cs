
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Core.Entities.Trips
{
    public class F03StagingTrip: IStagingData
    {
        [Key]
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Destination { get; set; }
        public string? Purpose { get; set; }

        // Status tracking
        public bool IsProcessed { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}
