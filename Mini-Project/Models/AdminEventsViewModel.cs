namespace Mini_Project.Models
{
    public class AdminEventsViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AttendeeLimit { get; set; }
        public int TotalRSVPs { get; set; }
        public List<RSVPViewModel> RSVPs { get; set; } = new List<RSVPViewModel>();
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public List<RatingViewModel> Ratings { get; set; } = new List<RatingViewModel>();
    }

    public class RSVPViewModel
    {
        public string UserId { get; set; }
    }

    public class CommentViewModel
    {
        public string Content { get; set; }
    }

    public class RatingViewModel
    {
        public int EventRating { get; set; }
        public int RecommendationRating { get; set; }
    }

}
