
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class EventDto
{
    [Required]
    [StringLength(100)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 9999.99)]
    public decimal Distance { get; set; }

    [Required]
    [RegularExpression("^(Run|Walk|Cycle)$")]
    public string EventType { get; set; } = string.Empty;
}
