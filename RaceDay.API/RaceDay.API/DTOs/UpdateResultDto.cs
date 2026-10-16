
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class UpdateResultDto
{
    [Required]
    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }
}
