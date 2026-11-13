
using System.ComponentModel.DataAnnotations;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests;

public class RegistrationValidationTests
{
    private static bool IsValid(RegisterDto dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        return Validator.TryValidateObject(
            dto,
            context,
            results,
            validateAllProperties: true);
    }

    [Fact]
    public void Register_WithValidDetails_PassesValidation()
    {
        var dto = new RegisterDto
        {
            FirstName = "Nomsa",
            LastName = "Mokoena",
            Email = "nomsa@gmail.com",
            Password = "RaceDay@2026",
            Role = "Organiser"
        };

        Assert.True(IsValid(dto));
    }

    [Fact]
    public void Register_WithInvalidEmail_FailsValidation()
    {
        var dto = new RegisterDto
        {
            FirstName = "Nomsa",
            LastName = "Mokoena",
            Email = "invalid-email",
            Password = "RaceDay@2026",
            Role = "Organiser"
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void Register_WithShortPassword_FailsValidation()
    {
        var dto = new RegisterDto
        {
            FirstName = "Nomsa",
            LastName = "Mokoena",
            Email = "nomsa@gmail.com",
            Password = "123",
            Role = "Organiser"
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void Register_WithMissingFirstName_FailsValidation()
    {
        var dto = new RegisterDto
        {
            FirstName = "",
            LastName = "Mokoena",
            Email = "nomsa@gmail.com",
            Password = "RaceDay@2026",
            Role = "Participant"
        };

        Assert.False(IsValid(dto));
    }
}
