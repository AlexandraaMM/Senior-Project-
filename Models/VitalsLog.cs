using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class VitalsLog
    {
        [Key]
        public int VitalID { get; set; }  // Entry for specific health measurements
        
        [Required]
        public int RecordId { get; set; }  
        
        [Required]
        public int PatientID { get; set; }  
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight_kg { get; set; }  // Used for BMI calculation
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height_cm { get; set; }  // Used for BMI calculation
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? BMI { get; set; }  // Calculated Body Mass Index
        
        public DateTime RecordedDate { get; set; } = DateTime.Now;  // When the vitals were recorded
        
        // Foreign key relationships
        [ForeignKey("RecordId")]
        public MedicalRecord? MedicalRecord { get; set; }  // Links to medical record
        
        [ForeignKey("PatientID")]
        public User? Patient { get; set; }
    }
}