using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Comment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
    }
}
