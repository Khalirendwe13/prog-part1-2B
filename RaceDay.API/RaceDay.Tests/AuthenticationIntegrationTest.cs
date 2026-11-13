
using System.Net;
using System.Net.Http.Json;
using RaceDay.Tests.Infrastructure;
using Xunit;

namespace RaceDay.Tests;

public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task Register_ValidOrganiser_ReturnsCreated()
    {
        using var factory = new RaceDayWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Moses",
                lastName = "Ramphabana",
                email = "moses@gmail.com",
                password = "RaceDay@moses",
                role = "Organiser"
            });

        Assert.Equal(HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var factory = new RaceDayWebApplicationFactory();
        using var client = factory.CreateClient();

        var user = new
        {
            firstName = "Ronaldo",
            lastName = "Goat",
            email = "ronaldo@gmail.com",
            password = "RaceDay@ronaldo",
            role = "Participant"
        };

        var first = await client.PostAsJsonAsync( "/api/auth/register", user);

        var second = await client.PostAsJsonAsync("/api/auth/register", user);

        Assert.Equal(HttpStatusCode.Created,first.StatusCode);

        Assert.Equal(HttpStatusCode.Conflict,second.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        using var factory = new RaceDayWebApplicationFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                firstName = "Thabo",
                lastName = "Messi",
                email = "thabo@gmail.com",
                password = "RaceDay@thabo",
                role = "Participant"
            });

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "thabo@gmail.com",
                password = "IncorrectPassword"
            });

        Assert.Equal(HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
    {
        using var factory = new RaceDayWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/events",
            new
            {
                eventName = "Test Marathon",
                description = "Testing authentication",
                eventDate = "2026-10-18",
                location = "Johannesburg",
                distance = 21.1,
                eventType = "Run"
            });

        Assert.Equal(HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
