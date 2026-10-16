
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using RaceDay.Tests.Infrastructure;
using Xunit;

namespace RaceDay.Tests;

public class DatabaseIsolationTests
    : IClassFixture<RaceDayWebApplicationFactory>
{
    private readonly RaceDayWebApplicationFactory _factory;

    public DatabaseIsolationTests(
        RaceDayWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void IntegrationTests_UseInMemoryDatabase()
    {
        // Start the application test host
        using var client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<RaceDayDbContext>();

        // Confirm EF Core uses InMemory
        Assert.True(db.Database.IsInMemory(),"Integration tests must use the InMemory provider.");

        // Confirm the seeded roles are available.
        Assert.Contains(db.Roles, role => role.RoleName == "Organiser");

        Assert.Contains(db.Roles,role => role.RoleName == "Participant");
    }
}
