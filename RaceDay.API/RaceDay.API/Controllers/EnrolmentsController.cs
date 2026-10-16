
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    // Participant enrols in an event category.
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] EnrolmentDto request)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        bool participantExists = await _db.Participants
            .AnyAsync(p => p.ParticipantID == participantId);

        if (!participantExists)
            return Forbid();

        var raceEvent = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == request.EventID);

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        if (raceEvent.EventDate.Date < DateTime.UtcNow.Date)
            return BadRequest(new
            {
                message = "Cannot enrol in a past event."
            });

        bool categoryExists = await _db.Categories
            .AnyAsync(c =>
                c.EventID == request.EventID &&
                c.CategoryID == request.CategoryID);

        if (!categoryExists)
            return BadRequest(new
            {
                message = "Category does not belong to this event."
            });

        bool alreadyEnrolled = await _db.Enrolments
            .AnyAsync(e =>
                e.ParticipantID == participantId &&
                e.EventID == request.EventID);

        if (alreadyEnrolled)
            return Conflict(new
            {
                message = "You have already entered this event."
            });

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            EventID = request.EventID,
            CategoryID = request.CategoryID
        };

        _db.Enrolments.Add(enrolment);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "Enrolment could not be created. Check whether you are already enrolled."
            });
        }

        return CreatedAtAction(
            nameof(GetMyEnrolment),
            new { id = enrolment.EnrolmentID },
            new
            {
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.EventID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate
            });
    }

    // Participant views all their own enrolments.
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantID == participantId)
            .OrderByDescending(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EnrolmentDate,
                e.EventID,
                e.Event.EventName,
                e.Event.EventDate,
                e.CategoryID,
                e.Category.CategoryName
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // Participant views one of their enrolments.
    [HttpGet("my/{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolment(int id)
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var enrolment = await _db.Enrolments
            .AsNoTracking()
            .Where(e =>
                e.EnrolmentID == id &&
                e.ParticipantID == participantId)
            .Select(e => new
            {
                e.EnrolmentID,
                e.ParticipantID,
                e.EventID,
                e.Event.EventName,
                e.CategoryID,
                e.Category.CategoryName,
                e.EnrolmentDate
            })
            .FirstOrDefaultAsync();

        if (enrolment == null)
            return NotFound(new
            {
                message = "Enrolment not found."
            });

        return Ok(enrolment);
    }

    // Organiser views enrolments for their own event.
    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var raceEvent = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == eventId);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found."
            });

        if (raceEvent.OrganiserID != organiserId)
            return Forbid();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Where(e => e.EventID == eventId)
            .OrderBy(e => e.EnrolmentID)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EnrolmentDate,
                e.ParticipantID,
                e.Participant.User.FirstName,
                e.Participant.User.LastName,
                e.CategoryID,
                e.Category.CategoryName
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
