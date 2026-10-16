namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public Participant Participant { get; set; } = null!;

        public RaceEvent Event { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public RaceResult? Result { get; set; }
    }
}
