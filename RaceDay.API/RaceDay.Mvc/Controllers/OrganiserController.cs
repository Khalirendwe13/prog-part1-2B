using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Events()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.GetAsync("/api/events");
            var events = await response.Content.ReadFromJsonAsync<List<JsonElement>>() ?? new();
            return View(events);
        }
        [HttpPost]
        public async Task<IActionResult> AddResult(
    int eventId,
    int enrolmentID,
    string finishTime,
    int finishingPosition)
        {
            var client = CreateAuthenticatedApiClient();

            var data = new
            {
                enrolmentID,
                finishTime,
                finishingPosition
            };

            var response = await client.PostAsJsonAsync(
                "/api/results", data);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Result recorded successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Could not record result: {error}";
            }

            return RedirectToAction("EventResults", new { eventId });
        }

        public IActionResult CreateEvent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string description, DateTime eventDate, string location, double distance, string eventType)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var data = new
            {
                eventName,
                description,
                eventDate,
                location,
                distance,
                eventType
            };

            var response = await client.PostAsJsonAsync("/api/events", data);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Events");
            }

           
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Failed to create event: {errorContent}";
            return View();
        }

        public async Task<IActionResult> Categories(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var eventResponse = await client.GetAsync($"/api/events/{id}");
            var categoriesResponse = await client.GetAsync($"/api/events/{id}/categories");

            if (!eventResponse.IsSuccessStatusCode)
                return NotFound();

            ViewBag.EventId = id;
            ViewBag.EventName = (await eventResponse.Content.ReadFromJsonAsync<JsonElement>())
                                .GetProperty("eventName").GetString();

            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<JsonElement>>()
                             ?? new List<JsonElement>();

            return View(categories);
        }

        public async Task<IActionResult> Results()
        {
            var client = CreateAuthenticatedApiClient();

            var response = await client.GetAsync("/api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load events.";
                return View(new List<JsonElement>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<JsonElement>>();

            return View(events ?? new List<JsonElement>());
        }
        public async Task<IActionResult> EventResults(int eventId)
        {
            var client = CreateAuthenticatedApiClient();

            var response = await client.GetAsync(
                $"/api/enrolments/event/{eventId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = $"Failed to load enrolments: {error}";
                return View(new List<JsonElement>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<JsonElement>>() ?? new();

            ViewBag.EventId = eventId;

            return View(enrolments);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(
            int eventId,
            string categoryName,
            int? minimumAge,
            int? maximumAge,
            decimal? distance)
                {
                    var client = _httpClientFactory.CreateClient("RaceDayApi");

                    var data = new
                    {
                        categoryName = categoryName ?? "General",
                        minimumAge,
                        maximumAge,
                        distance
                    };

                    var response = await client.PostAsJsonAsync(
                        $"/api/events/{eventId}/categories", data);

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] =
                            $"Status: {(int)response.StatusCode} - {responseContent}";
                    }
                    else
                    {
                        TempData["Success"] = "Category added successfully!";
                    }

                    return RedirectToAction("Categories", new { id = eventId });
                }

     }
}