
using System.Security.Claims;

namespace RaceDay.API.Authentication;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            int? sessionUserId =
                context.Session.GetInt32("UserID");

            string? sessionRole =
                context.Session.GetString("Role");

            string? claimUserId =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            string? claimRole =
                context.User.FindFirstValue(
                    ClaimTypes.Role);

            if (sessionUserId == null ||
                sessionUserId.ToString() != claimUserId ||
                sessionRole != claimRole)
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(
                    new { message = "Session expired or invalid." });

                return;
            }
        }

        await _next(context);
    }
}
