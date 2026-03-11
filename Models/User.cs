using System.ComponentModel.DataAnnotations;

namespace PracticeFlow.Models
{
    public class User
    {
       
        public User()
        {
            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = string.Empty;
        }

        [Key]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }  
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; }
        
        public DateTime? DateOfBirth { get; set; }

        
        
    }
}