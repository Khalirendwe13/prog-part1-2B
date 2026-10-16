namespace RaceDay.API.Models
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public User User { get; set; } = null;
        public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>();
    }
}
