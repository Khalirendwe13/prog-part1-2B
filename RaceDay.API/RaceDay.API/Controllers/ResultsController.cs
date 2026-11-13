
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    // POST: api/results
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        [FromBody] ResultDto request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentID == request.EnrolmentID);

        if (enrolment == null)
            return NotFound(new
            {
                message = "Enrolment not found."
            });

        if (enrolment.Event.OrganiserID != organiserId)
            return Forbid();

        if (enrolment.Event.EventDate.Date > DateTime.UtcNow.Date)
            return BadRequest(new
            {
                message = "Results cannot be recorded before the event date."
            });

        bool resultExists = await _db.Results
            .AnyAsync(r =>
                r.EnrolmentID == request.EnrolmentID);

        if (resultExists)
            return Conflict(new
            {
                message = "This enrolment already has a result."
            });

        if (!IsValidFinishTime(request.FinishTime))
            return BadRequest(new
            {
                message = "Finish time must be greater than zero and less than 24 hours."
            });

        var result = new RaceResult
        {
            EnrolmentID = request.EnrolmentID,
            FinishTime = request.FinishTime!.Value,
            FinishingPosition = request.FinishingPosition
        };

        _db.Results.Add(result);

        await _db.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Result recorded successfully.",
            result.ResultID,
            result.EnrolmentID,
            result.FinishTime,
            result.FinishingPosition
        });
    }

    // PUT: api/results/1 
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        [FromBody] UpdateResultDto request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var result = await _db.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null)
            return NotFound(new
            {
                message = "Result not found."
            });

        if (result.Enrolment.Event.OrganiserID != organiserId)
            return Forbid();

        if (!IsValidFinishTime(request.FinishTime))
            return BadRequest(new
            {
                message = "Invalid finish time."
            });

        result.FinishTime = request.FinishTime!.Value;
        result.FinishingPosition = request.FinishingPosition;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Result updated successfully."
        });
    }

    // GET: api/results/event/1
    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventResults(int eventId)
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

        var results = await _db.Results
            .AsNoTracking()
            .Where(r => r.Enrolment.EventID == eventId)
            .OrderBy(r => r.FinishingPosition)
            .Select(r => new
            {
                r.ResultID,
                r.EnrolmentID,
                r.Enrolment.ParticipantID,
                r.Enrolment.Participant.User.FirstName,
                r.Enrolment.Participant.User.LastName,
                r.Enrolment.Category.CategoryName,
                r.FinishTime,
                r.FinishingPosition
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/results/my
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!TryGetUserId(out int participantId))
            return Unauthorized();

        var results = await _db.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment.ParticipantID == participantId)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .Select(r => new
            {
                r.ResultID,
                r.Enrolment.EventID,
                r.Enrolment.Event.EventName,
                r.Enrolment.Event.EventDate,
                r.Enrolment.Category.CategoryName,
                r.FinishTime,
                r.FinishingPosition
            })
            .ToListAsync();

        return Ok(results);
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse( User.FindFirstValue(ClaimTypes.NameIdentifier),out userId);
    }

    private static bool IsValidFinishTime(TimeSpan? finishTime)
    {
        return finishTime.HasValue && finishTime.Value > TimeSpan.Zero && finishTime.Value < TimeSpan.FromDays(1);
    }
}
