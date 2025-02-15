using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Mini_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Controllers
{
    public class VisitorController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<VisitorController> _logger;

        public VisitorController(ILogger<VisitorController> logger)
        {
            FirebaseConfig.InitializeFirebase();
            _firestoreDb = FirestoreDb.Create("w-mini-project");
            _logger = logger;
        }

        public IActionResult VisitorDashboard()
        {
            return View();
        }

        public IActionResult Rate()
        {
            return View();
        }

        public IActionResult RSVP()
        {
            return View();
        }

        public IActionResult Comment()
        {
            return View();
        }

        // Method to retrieve all visible events
        public async Task<IActionResult> ViewEvents()
        {
            var eventsCollection = _firestoreDb.Collection("events");
            var snapshot = await eventsCollection.GetSnapshotAsync();
            var events = new List<Event>();

            foreach (var document in snapshot.Documents)
            {
                var eventData = document.ToDictionary();
                // Check if the event's visibility is true
                if (eventData.ContainsKey("EventVisibility") && (bool)eventData["EventVisibility"])
                {
                    events.Add(new Event
                    {
                        Id = document.Id,
                        EventName = eventData.ContainsKey("EventName") ? eventData["EventName"].ToString() : "N/A",
                        Description = eventData.ContainsKey("Description") ? eventData["Description"].ToString() : "N/A",
                        Category = eventData.ContainsKey("Category") ? eventData["Category"].ToString() : "N/A",
                        Location = eventData.ContainsKey("Location") ? eventData["Location"].ToString() : "N/A",
                        StartTime = eventData.ContainsKey("StartTime") && eventData["StartTime"] is Timestamp startTime
                            ? startTime.ToDateTime()
                            : default(DateTime),
                        EndTime = eventData.ContainsKey("EndTime") && eventData["EndTime"] is Timestamp endTime
                            ? endTime.ToDateTime()
                            : default(DateTime),
                        RSVP_limit = eventData.ContainsKey("RSVP_limit") ? Convert.ToInt32(eventData["RSVP_limit"]) : 0,
                        EventVisibility = eventData.ContainsKey("EventVisibility") && (bool)eventData["EventVisibility"]
                    });
                }
            }

            if (!events.Any())
            {
                TempData["ErrorMessage"] = "No events available.";
            }

            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> RSVP(string eventId, string packageChoice)
        {
            var userId = User.Identity.Name; // Assuming the user is logged in and has a unique ID

            // Check if the user has already RSVP'd
            var existingRSVP = await _firestoreDb.Collection("RSVP")
                .WhereEqualTo("EventId", eventId)
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            if (existingRSVP.Documents.Any())
            {
                TempData["ErrorMessage"] = "You have already RSVP'd for this event!";
                return RedirectToAction("ViewAllEvents");
            }

            var rsvp = new RSVP
            {
                EventId = eventId,
                UserId = userId,
                PackageChoice = packageChoice
            };

            var rsvpRef = _firestoreDb.Collection("RSVP").Document();
            await rsvpRef.SetAsync(rsvp);

            TempData["SuccessMessage"] = "RSVP submitted successfully!";
            return RedirectToAction("ViewAllEvents");
        }

        [HttpPost]
        public async Task<IActionResult> RateEvent(string eventId, int ratingValue, int wouldRecommend)
        {
            var userId = User.Identity.Name;

            if (ratingValue < 1 || ratingValue > 5)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating between 1 and 5.";
                return RedirectToAction("ViewAllEvents");
            }

            var rating = new Rating
            {
                EventId = eventId,
                UserId = userId,
                RatingValue = ratingValue,
                WouldRecommend = wouldRecommend
            };

            var ratingRef = _firestoreDb.Collection("Ratings").Document();
            await ratingRef.SetAsync(rating);

            TempData["SuccessMessage"] = "Rating submitted successfully!";
            return RedirectToAction("ViewAllEvents");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string eventId, string content)
        {
            var userId = User.Identity.Name;

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("ViewAllEvents");
            }

            var comment = new EventComment
            {
                EventId = eventId,
                VisitorId = userId,
                CommentText = content
            };

            var commentRef = _firestoreDb.Collection("Comments").Document();
            await commentRef.SetAsync(comment);

            TempData["SuccessMessage"] = "Comment submitted successfully!";
            return RedirectToAction("ViewAllEvents");
        }
    }
}
