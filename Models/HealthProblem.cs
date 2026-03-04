using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class HealthProblem
    {
        [Key]
        public int ProblemID { get; set; }  // Stores the diagnosis
        
        [Required]
        public int PatientID { get; set; }  
        
        [Required]
        [StringLength(200)]
        public string ProblemDescription { get; set; } = string.Empty;  // Description of the health issue
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";  
        
        public DateTime DiagnosedDate { get; set; } = DateTime.Now;  // When the problem was diagnosed
        
        public DateTime? ResolvedDate { get; set; }  // When the problem was resolved
        
        // Foreign key relationship
        [ForeignKey("PatientID")]
        public User? Patient { get; set; }
    }
}