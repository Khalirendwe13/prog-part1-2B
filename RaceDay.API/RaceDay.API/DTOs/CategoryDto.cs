
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CategoryDto : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0, 150)]
    public int? MinimumAge { get; set; }

    [Range(0, 150)]
    public int? MaximumAge { get; set; }

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal? Distance { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MinimumAge.HasValue && MaximumAge.HasValue &&
            MinimumAge > MaximumAge)
        {
            yield return new ValidationResult(
                "MinimumAge cannot exceed MaximumAge.",
                new[] { nameof(MinimumAge), nameof(MaximumAge)});
        }

        if (!MinimumAge.HasValue &&
            !MaximumAge.HasValue &&
            !Distance.HasValue)
        {
            yield return new ValidationResult(
                "Provide an age range or a distance.",
                new[]
                {
                    nameof(MinimumAge),
                    nameof(MaximumAge),
                    nameof(Distance)
                });
        }
    }
}
