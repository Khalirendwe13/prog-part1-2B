
using Microsoft.AspNetCore.Antiforgery;

namespace RaceDay.API.Authentication;

public class AntiforgeryValidationMiddleware
{
    private readonly RequestDelegate _next;

    public AntiforgeryValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
        IAntiforgery antiforgery)
    {
        bool changesData = HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method)
            || HttpMethods.IsDelete(context.Request.Method);

        bool isProtected = context.User.Identity?.IsAuthenticated == true;

        if (changesData && isProtected)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid or missing CSRF token."
                });
                return;
            }
        }

        await _next(context);
    }
}
