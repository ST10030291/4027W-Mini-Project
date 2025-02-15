using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class Festival
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); 

        [Required]
        public string FestivalName { get; set; }  

        [Required]
        public string Location { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // List of associated events that can be added to this festival
        public List<Event> Events { get; set; } = new List<Event>();
    }

}
