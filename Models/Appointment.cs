using System.ComponentModel.DataAnnotations;

namespace PracticeFlow.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Scheduled"; 
        // Navigation properties
        public User? Patient { get; set; }
        public User? Doctor { get; set; }
    }
}