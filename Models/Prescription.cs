using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }  // Unique prescription identifier
        
        [Required]
        public int RecordId { get; set; }  // Created during medical record
        
        [Required]
        public int PatientID { get; set; }  // Patient receiving the prescription
        
        [Required]
        [StringLength(200)]
        public string MedicationName { get; set; } = string.Empty;  // Drug name
        
        [Required]
        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;  
        
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;  // Treatment start
        
        public DateTime? EndDate { get; set; }  // Treatment duration
        
        public string Instructions { get; set; } = string.Empty;  // Doctor notes
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;  // When prescription was created
        
        // Foreign key relationships
        [ForeignKey("RecordId")]
        public MedicalRecord? MedicalRecord { get; set; }  
        
        [ForeignKey("PatientID")]
        public User? Patient { get; set; }
    }
}