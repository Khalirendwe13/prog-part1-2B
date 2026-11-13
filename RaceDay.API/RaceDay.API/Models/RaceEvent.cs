namespace RaceDay.API.Models
{
    public class RaceEvent
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal Distance { get; set; }

        public string EventType { get; set; } = string.Empty;

        // Public URL of the event banner stored in Azure Blob Storage.
        public string? BannerImageUrl { get; set; }

        public int OrganiserID { get; set; }

        public Organiser Organiser { get; set; } = null!;

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}
