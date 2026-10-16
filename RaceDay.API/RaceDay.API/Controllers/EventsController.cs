
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    // GET: api/events
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _db.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserID
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/1
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(int id)
    {
        var raceEvent = await _db.Events
            .AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserID
            })
            .FirstOrDefaultAsync();

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found."
            });

        return Ok(raceEvent);
    }

    // POST: api/events
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] EventDto request)
    {
        int organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var organiserExists = await _db.Organisers
            .AnyAsync(o => o.OrganiserID == organiserId);

        if (!organiserExists)
            return Forbid();

        var raceEvent = new RaceEvent
        {
            EventName = request.EventName.Trim(),
            Description = request.Description.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = request.EventType,
            OrganiserID = organiserId
        };

        _db.Events.Add(raceEvent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EventID },
            new
            {
                raceEvent.EventID,
                raceEvent.EventName,
                raceEvent.Description,
                raceEvent.EventDate,
                raceEvent.Location,
                raceEvent.Distance,
                raceEvent.EventType
            });
    }

    // PUT: api/events/1
    // Only the Organiser who owns the event can edit it.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] EventDto request)
    {
        int organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _db.Events.FindAsync(id);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found."
            });

        if (raceEvent.OrganiserID != organiserId)
            return Forbid();

        raceEvent.EventName = request.EventName.Trim();
        raceEvent.Description = request.Description.Trim();
        raceEvent.EventDate = request.EventDate;
        raceEvent.Location = request.Location.Trim();
        raceEvent.Distance = request.Distance;
        raceEvent.EventType = request.EventType;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully."
        });
    }

    // DELETE: api/events/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        int organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _db.Events.FindAsync(id);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found."
            });

        if (raceEvent.OrganiserID != organiserId)
            return Forbid();

        bool hasEnrolments = await _db.Enrolments
            .AnyAsync(e => e.EventID == id);

        if (hasEnrolments)
            return Conflict(new
            {
                message =
                    "Cannot delete an event with enrolments."
            });

        var categories = await _db.Categories
            .Where(c => c.EventID == id)
            .ToListAsync();

        _db.Categories.RemoveRange(categories);
        _db.Events.Remove(raceEvent);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
