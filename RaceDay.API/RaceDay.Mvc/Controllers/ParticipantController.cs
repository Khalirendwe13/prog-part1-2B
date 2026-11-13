using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private HttpClient CreateAuthenticatedApiClient()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var cookie = Request.Headers.Cookie.ToString();

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cookie", cookie);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            // Forwarding auth cookir to  API
            var response = await client.GetAsync("/api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load events";
                return View(new List<JsonElement>());
            }

            var events = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
            return View(events ?? new List<JsonElement>());
        }

        public async Task<IActionResult> MyEvents()
        {
            var client = CreateAuthenticatedApiClient();
            var response = await client.GetAsync("/api/enrolments/my");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load your enrolments";
                return View(new List<JsonElement>());
            }

            var enrolments = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
            return View(enrolments ?? new List<JsonElement>());
        }

        public async Task<IActionResult> MyResults()
        {
            var client = CreateAuthenticatedApiClient();
            var response = await client.GetAsync("/api/results/my");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load results";
                return View(new List<JsonElement>());
            }

            var results = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
            return View(results ?? new List<JsonElement>());
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var eventResponse = await client.GetAsync($"/api/events/{id}");
            var categoriesResponse = await client.GetAsync($"/api/events/{id}/categories");

            if (!eventResponse.IsSuccessStatusCode)
                return NotFound();

            ViewBag.Event = await eventResponse.Content.ReadFromJsonAsync<JsonElement>();
            ViewBag.Categories = await categoriesResponse.Content.ReadFromJsonAsync<List<JsonElement>>() ?? new();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var client = CreateAuthenticatedApiClient();
            var data = new
            {
                eventID = eventId,
                categoryID = categoryId
            };

            var response = await client.PostAsJsonAsync("/api/enrolments", data);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Successfully enrolled!";
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    TempData["Error"] = error.GetProperty("message").GetString();
                }
                else
                {
                    TempData["Error"] = "Something went wrong while enrolling.";
                }
            }

            return RedirectToAction("MyEvents");
        }

    }
}