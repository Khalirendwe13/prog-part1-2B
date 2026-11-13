namespace RaceDay.API.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
