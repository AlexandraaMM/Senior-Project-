using System;
using System.ComponentModel.DataAnnotations;

namespace PracticeFlow.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}