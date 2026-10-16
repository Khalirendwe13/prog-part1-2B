
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ProfileController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the logged-in user's profile information.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var profile = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId)
            .Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                Role = u.Role.RoleName,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (profile == null)
            return NotFound(new
            {
                message = "User profile not found."
            });

        return Ok(profile);
    }

    /// Updates the logged-in user's personal information.
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto request)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        string firstName = request.FirstName.Trim();
        string lastName = request.LastName.Trim();
        string email = request.Email.Trim().ToLowerInvariant();

        if (firstName.Length < 2 || lastName.Length < 2)
        {
            return BadRequest(new
            {
                message = "First and last names must contain at least two characters."
            });
        }

        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new
            {
                message = "User profile not found."
            });

        bool emailTaken = await _db.Users
            .AnyAsync(u =>
                u.Email == email &&
                u.UserID != userId);

        if (emailTaken)
            return Conflict(new
            {
                message = "Email address is already registered."
            });

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
