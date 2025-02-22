using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
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

        public async Task<IActionResult> VisitorDashboard()
        {
            try
            {
                var eventsCollection = _firestoreDb.Collection("events");
                var snapshot = await eventsCollection.GetSnapshotAsync();

                var eventsList = new List<AdminEventsViewModel>();

                foreach (var doc in snapshot.Documents)
                {
                    doc.TryGetValue("EventName", out string eventName);
                    doc.TryGetValue("Location", out string location);
                    doc.TryGetValue("StartTime", out Timestamp startTime);
                    doc.TryGetValue("EndTime", out Timestamp endTime);
                    doc.TryGetValue("RSVP_limit", out int attendeeLimit);

                    // Fetch related RSVP records
                    var rsvpCollection = _firestoreDb.Collection("RSVPs").WhereEqualTo("EventName", doc.Id);
                    var rsvpSnapshot = await rsvpCollection.GetSnapshotAsync();
                    var rsvpList = rsvpSnapshot.Documents.Select(rsvpDoc => new RSVPViewModel
                    {
                        UserId = rsvpDoc.GetValue<string>("UserId")
                    }).ToList();
                    int totalRsvps = rsvpList.Count;

                    // Fetch related Comments
                    var commentsCollection = _firestoreDb.Collection("comments").WhereEqualTo("EventId", doc.Id);
                    var commentsSnapshot = await commentsCollection.GetSnapshotAsync();
                    var commentsList = commentsSnapshot.Documents.Select(commentDoc => new CommentViewModel
                    {
                        Content = commentDoc.GetValue<string>("Content")
                    }).ToList();

                    // Fetch related Ratings
                    var ratingsCollection = _firestoreDb.Collection("Ratings").WhereEqualTo("EventId", doc.Id);
                    var ratingsSnapshot = await ratingsCollection.GetSnapshotAsync();
                    var ratingsList = ratingsSnapshot.Documents.Select(ratingDoc => new RatingViewModel
                    {
                        EventRating = ratingDoc.GetValue<int>("EventRating"),
                        RecommendationRating = ratingDoc.GetValue<int>("RecommendationRating")
                    }).ToList();

                    eventsList.Add(new AdminEventsViewModel
                    {
                        Id = doc.Id,
                        Name = eventName ?? "N/A",
                        Location = location ?? "N/A",
                        StartTime = startTime.ToDateTime(),
                        EndTime = endTime.ToDateTime(),
                        AttendeeLimit = attendeeLimit,
                        TotalRSVPs = totalRsvps,
                        RSVPs = rsvpList,
                        Comments = commentsList,
                        Ratings = ratingsList
                    });
                }

                return View(eventsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading events: {ex.Message}";
                return View(new List<AdminEventsViewModel>());
            }
        }

        public async Task<IActionResult> Comment()
        {
            try
            {
                var eventsQuery = _firestoreDb.Collection("events")
                    .WhereEqualTo("EventVisibility", true);

                var eventSnapshot = await eventsQuery.GetSnapshotAsync();
                var eventsList = eventSnapshot.Documents.Select(doc => new EventViewModel
                {
                    Id = doc.Id,
                    Name = doc.GetValue<string>("EventName")
                }).ToList();

                return View(eventsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving events: {ex.Message}";
                return RedirectToAction("ViewAllEvents");
            }
        }

        public async Task<IActionResult> Rate()
        {
            try
            {
                var eventsQuery = _firestoreDb.Collection("events")
                    .WhereEqualTo("EventVisibility", true);

                var eventSnapshot = await eventsQuery.GetSnapshotAsync();
                var eventsList = eventSnapshot.Documents.Select(doc => new EventViewModel
                {
                    Id = doc.Id,
                    Name = doc.GetValue<string>("EventName")
                }).ToList();

                return View(eventsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving events: {ex.Message}";
                return RedirectToAction("ViewAllEvents");
            }
        }

        // Save Ratings
        [HttpPost]
        public async Task<IActionResult> SubmitRating(string email, string eventId, int eventRating, int recommendRating)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                TempData["Error"] = "Event selection is required.";
                return RedirectToAction("Rate");
            }

            // Get the currently logged-in user's email
            var userEmail = email;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction("Rate");
            }

            // Get UserId from Firestore based on Email
            Query userQuery = _firestoreDb.Collection("Users").WhereEqualTo("Email", userEmail);
            QuerySnapshot userSnapshot = await userQuery.GetSnapshotAsync();
            if (!userSnapshot.Any())
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Rate");
            }

            var userId = userSnapshot.Documents.First().Id;

            // Create rating object
            var ratingData = new
            {
                Id = Guid.NewGuid().ToString(),
                EventId = eventId,
                UserId = userId,
                EventRating = eventRating,
                RecommendationRating = recommendRating,
                Timestamp = Timestamp.GetCurrentTimestamp()
            };

            // Save to Firestore
            DocumentReference ratingRef = _firestoreDb.Collection("Ratings").Document();
            await ratingRef.SetAsync(ratingData);

            TempData["Success"] = "Thank you for your rating!";
            return RedirectToAction("Rate");
        }

        // Save a comment
        [HttpPost]
        public async Task<IActionResult> SubmitComment(string email, string eventName, string content)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(content))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("Comment");
            }

            // Fetch user ID from Firebase based on email
            var userQuery = _firestoreDb.Collection("Users").WhereEqualTo("Email", email).Limit(1);
            var userSnapshot = await userQuery.GetSnapshotAsync();
            var user = userSnapshot.Documents.FirstOrDefault();

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Comment");
            }

            string userId = user.Id;

            // Create the comment object
            var comment = new Comment
            {
                EventId = eventName, 
                UserId = userId, 
                Content = content 
            };

            // Save the comment to Firestore
            var commentRef = _firestoreDb.Collection("comments").Document(comment.Id);
            await commentRef.SetAsync(comment);

            TempData["SuccessMessage"] = "Comment submitted successfully!";
            return RedirectToAction("Comment");
        }


        // Create an RSVP
        [HttpPost]
        public async Task<IActionResult> SubmitRSVP(string email, string eventName, string packageChoice)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(packageChoice))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("RSVP");
            }

            // Search for the user by email
            var usersSnapshot = await _firestoreDb.Collection("Users")
                .WhereEqualTo("Email", email)
                .GetSnapshotAsync();

            var userDoc = usersSnapshot.Documents.FirstOrDefault();
            if (userDoc == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("RSVP");
            }

            string userId = userDoc.Id;

            var rsvp = new RSVP
            {
                EventName = eventName,
                UserId = userId,
                PackageChoice = packageChoice
            };

            // Save the RSVP in Firestore
            var rsvpRef = _firestoreDb.Collection("RSVPs").Document(rsvp.Id);
            await rsvpRef.SetAsync(rsvp);

            TempData["SuccessMessage"] = "RSVP confirmed successfully!";
            return RedirectToAction("VisitorDashboard", "Visitor");
        }

        public async Task<IActionResult> RSVP()
        {
            try
            {
                var eventsQuery = _firestoreDb.Collection("events")
                    .WhereEqualTo("EventVisibility", true);

                var eventSnapshot = await eventsQuery.GetSnapshotAsync();
                var eventsList = eventSnapshot.Documents.Select(doc => new EventViewModel
                {
                    Id = doc.Id,
                    Name = doc.GetValue<string>("EventName")
                }).ToList();

                return View(eventsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving events: {ex.Message}";
                return RedirectToAction("ViewAllEvents");
            }
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
    }
}
