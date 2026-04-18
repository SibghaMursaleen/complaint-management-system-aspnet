using System.ComponentModel.DataAnnotations;

namespace SIBA.ComplaintSystem.Models
{
    public class ComplaintArchive
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ComplaintTitle { get; set; } = string.Empty;

        [Required]
        public string FullTranscript { get; set; } = string.Empty;

        public DateTime ResolvedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string ResolvedBy { get; set; } = string.Empty;
    }
}
