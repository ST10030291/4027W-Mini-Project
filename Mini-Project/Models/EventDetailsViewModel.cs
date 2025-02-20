namespace Mini_Project.Models
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; }
        public List<RSVP> RSVPs { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Rating> Ratings { get; set; }
        public double AverageRating { get; set; }
        public double RecommendRating { get; set; }
    }
}
