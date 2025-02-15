using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class RSVP
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string PackageChoice { get; set; }  // Basic, Drinks, Food
    }
}
