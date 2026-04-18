using System.ComponentModel.DataAnnotations;

namespace SIBA.ComplaintSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Student" or "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ProfilePicturePath { get; set; }
    }
}
