using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class EventComment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EventId { get; set; } // Foreign key linking to Event

        [Required]
        public string VisitorId { get; set; } // Foreign key linking to Visitor 

        [Required]
        [MaxLength(500)]
        public string CommentText { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
