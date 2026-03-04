using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeFlow.Models
{
    public class TreatmentGoal
    {
        [Key]
        public int GoalID { get; set; }  
        
        [Required]
        public int ProblemID { get; set; }  
        
        [Required]
        [StringLength(500)]
        public string GoalDescription { get; set; } = string.Empty;  // What the patient aims to achieve
        
        [Range(0, 100)]
        public int ProgressPercent { get; set; } = 0;  
        
        public DateTime SetDate { get; set; } = DateTime.Now;  // When the goal was set
        
        public DateTime? TargetDate { get; set; }  // Target date for achieving the goal
        
        // Foreign key relationship
        [ForeignKey("ProblemID")]
        public HealthProblem? HealthProblem { get; set; }
    }
}