using System.ComponentModel.DataAnnotations;

namespace SIBA.ComplaintSystem.Models
{
    public class ComplaintMessage
    {
        public int Id { get; set; }

        [Required]
        public int ComplaintId { get; set; }
        public Complaint? Complaint { get; set; }

        [Required]
        public int SenderId { get; set; }
        public User? Sender { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
