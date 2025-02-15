using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Rating
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; }
        public string UserId { get; set; }
        public int RatingValue { get; set; }  // 1-5 stars
        public int WouldRecommend { get; set; } // 1-5 stars
    }
}
