using System.ComponentModel.DataAnnotations;

namespace SIBA.ComplaintSystem.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // Academic, Administration, Hostel/Transport, IT, Others

        public string Status { get; set; } = "Pending"; // Pending, In Progress, Resolved

        // -- New Fields from Spec --
        [StringLength(50)]
        public string? StudentId { get; set; }

        [StringLength(100)]
        public string? StudentDepartment { get; set; }

        [StringLength(50)]
        public string? Semester { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        [StringLength(100)]
        public string? TargetDepartment { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        public DateTime? IncidentDate { get; set; }

        [StringLength(500)]
        public string? AttachmentPath { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Link with User
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
