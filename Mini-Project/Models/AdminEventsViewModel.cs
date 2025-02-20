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
    }
}
