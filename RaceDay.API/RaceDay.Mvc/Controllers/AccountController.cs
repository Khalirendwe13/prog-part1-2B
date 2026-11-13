using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var loginData = new { email, password };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginData);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            // Capture the cookies from the API
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    // Store 
                    Response.Headers.Append("Set-Cookie", cookie);
                }
            }

            var user = await response.Content.ReadFromJsonAsync<JsonElement>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.GetProperty("userId").ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, user.GetProperty("role").GetString() ?? "Participant"),
                new Claim(ClaimTypes.Name, email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            var role = user.GetProperty("role").GetString();

            if (role == "Organiser")
                return RedirectToAction("Dashboard", "Organiser");

            return RedirectToAction("Index", "Participant");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string role)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var registerData = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                role = role
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", registerData);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Registration failed: {errorContent}";
                return View();
            }

            TempData["Success"] = "Account created successfully. Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}