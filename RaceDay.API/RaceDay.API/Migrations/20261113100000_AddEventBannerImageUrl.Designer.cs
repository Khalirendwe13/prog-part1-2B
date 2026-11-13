// Migration metadata is maintained by Entity Framework Core.
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaceDay.API.Data;

#nullable disable

namespace RaceDay.API.Migrations
{
    [DbContext(typeof(RaceDayDbContext))]
    [Migration("20261113100000_AddEventBannerImageUrl")]
    partial class AddEventBannerImageUrl
    {
    }
}
