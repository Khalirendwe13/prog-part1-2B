
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class ResultDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    [Required]
    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }
}
