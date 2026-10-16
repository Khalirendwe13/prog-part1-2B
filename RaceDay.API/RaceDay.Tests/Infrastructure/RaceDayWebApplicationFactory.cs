
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Microsoft.AspNetCore.TestHost;

namespace RaceDay.Tests.Infrastructure;

public class RaceDayWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"RaceDayTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        // Prevent production database seeding.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            // Use EF Core InMemory database.
            services.AddDbContext<RaceDayDbContext>(
                options =>
                    options.UseInMemoryDatabase(_databaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<RaceDayDbContext>();

        db.Database.EnsureCreated();

        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { RoleName = "Organiser" },
                new Role { RoleName = "Participant" }
            );

            db.SaveChanges();
        }

        return host;
    }
}
