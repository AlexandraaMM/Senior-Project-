using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class VitalsLog
    {
        [Key]
        public int VitalID { get; set; }  
        
        public int? RecordId { get; set; }  
        [Required]
        public int PatientID { get; set; }  
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight_kg { get; set; }  // Patient weight in kilograms
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height_cm { get; set; }  // Patient height in centimeters
        
        public DateTime RecordedDate { get; set; } = DateTime.Now;  // When the vitals were recorded
        
        // Foreign key relationships
        [ForeignKey("RecordId")]
        public MedicalRecord? MedicalRecord { get; set; }  
        
        [ForeignKey("PatientID")]
        public User? Patient { get; set; }
        
        // BMI is calculated
        [NotMapped]  
        public decimal? BMI
        {
            get
            {
                // Calculate BMI only if both weight and height are available
                if (Weight_kg.HasValue && Height_cm.HasValue && Height_cm.Value > 0)
                {
                    
                    decimal heightInMeters = Height_cm.Value / 100m;  // Convert cm to meters
                    return Math.Round(Weight_kg.Value / (heightInMeters * heightInMeters), 2);
                }
                return null;  
            }
        }
        
        [NotMapped]
        public string BMICategory
        {
            get
            {
                if (!BMI.HasValue) return "N/A";
                
                if (BMI < 18.5m) return "Underweight";
                if (BMI < 25m) return "Normal";
                if (BMI < 30m) return "Overweight";
                return "Obese";
            }
        }
    }
}