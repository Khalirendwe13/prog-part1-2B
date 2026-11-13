namespace RaceDay.API.Models
{
    public class RaceResult
    {
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishingPosition { get; set; }

        public Enrolment Enrolment { get; set; } = null!;
    }
}
