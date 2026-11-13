
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// Lists the available categories for an event.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        bool eventExists = await _db.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryID)
            .Select(c => new
            {
                c.CategoryID,
                c.EventID,
                c.CategoryName,
                c.MinimumAge,
                c.MaximumAge,
                c.Distance
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// Returns one category belonging to an event.
    [HttpGet("{categoryId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(
        int eventId, int categoryId)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId &&
                        c.CategoryID == categoryId)
            .Select(c => new
            {
                c.CategoryID,
                c.EventID,
                c.CategoryName,
                c.MinimumAge,
                c.MaximumAge,
                c.Distance
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    /// Creates a category for an Organiser's event.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        int eventId, [FromBody] CategoryDto request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var raceEvent = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == eventId);

        if (raceEvent == null)
            return NotFound(new { message = "Event not found." });

        if (raceEvent.OrganiserID != organiserId)
            return Forbid();

        var category = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            MinimumAge = request.MinimumAge,
            MaximumAge = request.MaximumAge,
            Distance = request.Distance
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new
            {
                eventId,
                categoryId = category.CategoryID
            },
            new
            {
                category.CategoryID,
                category.EventID,
                category.CategoryName,
                category.MinimumAge,
                category.MaximumAge,
                category.Distance
            });
    }

    /// Updates a category owned by the logged in Organiser.
    [HttpPut("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(
        int eventId,
        int categoryId,
        [FromBody] CategoryDto request)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c =>
                c.EventID == eventId &&
                c.CategoryID == categoryId);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.Event.OrganiserID != organiserId)
            return Forbid();

        bool hasEnrolments = await _db.Enrolments
            .AnyAsync(e => e.CategoryID == categoryId &&
                           e.EventID == eventId);

        if (hasEnrolments)
            return Conflict(new
            {
                message = "Cannot modify a category with enrolments."
            });

        category.CategoryName = request.CategoryName.Trim();
        category.MinimumAge = request.MinimumAge;
        category.MaximumAge = request.MaximumAge;
        category.Distance = request.Distance;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Category updated successfully." });
    }

    /// Deletes an unused category belonging to the Organiser.
    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(
        int eventId, int categoryId)
    {
        if (!TryGetUserId(out int organiserId))
            return Unauthorized();

        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c =>
                c.EventID == eventId &&
                c.CategoryID == categoryId);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.Event.OrganiserID != organiserId)
            return Forbid();

        bool hasEnrolments = await _db.Enrolments
            .AnyAsync(e => e.CategoryID == categoryId &&
                           e.EventID == eventId);

        if (hasEnrolments)
            return Conflict(new
            {
                message = "Cannot delete a category with enrolments."
            });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
