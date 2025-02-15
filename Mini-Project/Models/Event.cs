using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Event
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EventName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }
        public string Location { get; set; }
        public bool EventVisibility { get; set; } = true;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RSVP_limit { get; set; }
    }

}