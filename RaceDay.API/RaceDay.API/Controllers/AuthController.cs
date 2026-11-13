
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    //Registers a new Organiser or Participant
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto request)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        string roleName = request.Role.Trim();

        if (roleName != "Organiser" &&
            roleName != "Participant")
        {
            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new
            {
                message = "Email is already registered."
            });
        }

        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);

        if (role == null)
        {
            return StatusCode(500, new
            {
                message = "Roles have not been configured."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            RoleID = role.RoleID
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user, request.Password);

        if (roleName == "Organiser")
        {
            user.Organiser = new Organiser();
        }
        else
        {
            user.Participant = new Participant();
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Registration successful.",
            userId = user.UserID,
            role = roleName
        });
    }

    //Authenticates a user and creates a session
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto request)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        // Store identity in the server-side session.
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString(
            "Role", user.Role.RoleName);

        // Authentication cookie carries role claims.
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserID.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role.RoleName)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(new
        {
            message = "Login successful.",
            userId = user.UserID,
            role = user.Role.RoleName
        });
    }

    //Ends the authenticated user's session.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "Logout successful."
        });
    }

    //Returns the currently authenticated user.
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            email = User.Identity?.Name,
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
