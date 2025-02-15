using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class EventRating
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EventId { get; set; } // Foreign key linking to Event

        [Required]
        public string VisitorId { get; set; } // Foreign key linking to Visitor 

        [Range(1, 5)]
        public int Rating { get; set; } // 1 to 5 stars

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}