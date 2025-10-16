using System.ComponentModel.DataAnnotations;


namespace Entity.Entities
{
    public class LeaveRequest
    {

        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required, MaxLength(20)]
        public string LeaveType { get; set; } // Annual, Sick, Unpaid

        [Required, MaxLength(500)]
        public string Reason { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? ManagerComments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public virtual Employee Employee { get; set; }
    }

}
