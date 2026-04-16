using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }  // Tracks individual clinical encounters
        
        [Required]
        public int PatientId { get; set; }  // Identifies which patient was seen
        
        [Required]
        public int DoctorId { get; set; }  // Identifies which doctor conducted the visit

        public int? AppointmentId { get; set; }  // Links to appointment if created from Record Visit (nullable for direct creation)
    
        [Required]
        public DateTime RecordDate { get; set; } = DateTime.Now;  
        public ICollection<Prescription>? Prescriptions { get; set; }

        
        [StringLength(200)]
        public string ReasonForVisit { get; set; } = string.Empty;  
        
        public string? Diagnosis { get; set; } = string.Empty;  // Doctor's diagnosis
        
        public string? Treatment { get; set; } = string.Empty;  // Treatment plan
        
        
        public string? Notes { get; set; } = string.Empty;  
        
        // Foreign key relationships
        [ForeignKey("PatientId")]
        public User? Patient { get; set; }
        
        [ForeignKey("DoctorId")]
        public User? Doctor { get; set; }
    }
}