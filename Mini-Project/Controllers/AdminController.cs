using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Mini_Project.Models;

namespace Mini_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<AdminController> _logger;
        public AdminController(ILogger<AdminController> logger)
        {
            FirebaseConfig.InitializeFirebase();
            _firestoreDb = FirestoreDb.Create("w-mini-project");
            _logger = logger;

        }


        // Display all Comments for an Event
        // Display all Ratings for an Event
        // Display all RSVPs for an Event
        public async Task<IActionResult> EventDetails()
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

        // Method to display list of all users (only Visitors)
        public async Task<IActionResult> ViewAllUsers()
        {
            var usersCollection = _firestoreDb.Collection("Users");
            var snapshot = await usersCollection.GetSnapshotAsync();
            var users = new List<UserDetails>();

            foreach (var document in snapshot.Documents)
            {
                var user = document.ToDictionary();
                var role = user.ContainsKey("Role") ? user["Role"].ToString() : "Visitor";

                // Only add users with the "Visitor" role
                if (role.Equals("Visitor", StringComparison.OrdinalIgnoreCase))
                {
                    users.Add(new UserDetails
                    {
                        UID = document.Id,
                        Username = user.ContainsKey("Username") ? user["Username"].ToString() : "N/A",
                        Email = user.ContainsKey("Email") ? user["Email"].ToString() : "N/A",
                        Age = user.ContainsKey("Age") ? Convert.ToInt32(user["Age"]) : 0,
                        Gender = user.ContainsKey("Gender") ? user["Gender"].ToString() : "N/A",
                        Province = user.ContainsKey("Province") ? user["Province"].ToString() : "N/A",
                        FestivalAttendanceAmt = user.ContainsKey("FestivalAttendanceAmt") ? Convert.ToInt32(user["FestivalAttendanceAmt"]) : 0,
                        DateCreated = user.ContainsKey("DateCreated") && user["DateCreated"] is Timestamp timestamp
                                      ? timestamp.ToDateTime()
                                      : DateTime.MinValue,
                        Role = role
                    });
                }
            }

            if (!users.Any())
            {
                TempData["ErrorMessage"] = "No visitors found.";
            }

            return View(users);
        }

        // Method to Edit Event
        [HttpPost]
        public async Task<IActionResult> EditEventDetails(Event eventDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(eventDetails.EventName) || string.IsNullOrEmpty(eventDetails.Description) ||
                    string.IsNullOrEmpty(eventDetails.Category) || string.IsNullOrEmpty(eventDetails.Location) ||
                    eventDetails.StartTime == default(DateTime) || eventDetails.EndTime == default(DateTime) ||
                    eventDetails.RSVP_limit <= 0)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return RedirectToAction("EditEvent");
                }

                // Convert StartTime and EndTime to UTC
                var startTimeUtc = eventDetails.StartTime.ToUniversalTime();
                var endTimeUtc = eventDetails.EndTime.ToUniversalTime();

                // Check if event with same name exists in Firestore
                Query eventQuery = _firestoreDb.Collection("events")
                    .WhereEqualTo("EventName", eventDetails.EventName);

                QuerySnapshot eventSnapshot = await eventQuery.GetSnapshotAsync();

                if (eventSnapshot.Documents.Count > 0)
                {
                    // Event exists, get the document ID and update it
                    DocumentSnapshot existingEvent = eventSnapshot.Documents.First();
                    string existingEventId = existingEvent.Id;

                    DocumentReference eventRef = _firestoreDb.Collection("events").Document(existingEventId);

                    var updatedData = new Dictionary<string, object>
            {
                { "Description", eventDetails.Description },
                { "Category", eventDetails.Category },
                { "Location", eventDetails.Location },
                { "StartTime", startTimeUtc },
                { "EndTime", endTimeUtc },
                { "RSVP_limit", eventDetails.RSVP_limit },
                { "EventVisibility", eventDetails.EventVisibility }
            };

                    await eventRef.UpdateAsync(updatedData);

                    TempData["SuccessMessage"] = "Event updated successfully!";
                    RedirectToAction("AdminDashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found. Please check the event name.";
                    RedirectToAction("AdminDashboard");
                }

                return RedirectToAction("EditEvent");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the event: " + ex.Message;
                return RedirectToAction("EditEvent");
            }
        }

        // Method to save Event details
        [HttpPost]
        public async Task<IActionResult> SaveEventDetails(Event eventDetails)
        {
            try
            {
                // Check if all required fields are filled in
                if (string.IsNullOrEmpty(eventDetails.EventName) || string.IsNullOrEmpty(eventDetails.Description) ||
                    string.IsNullOrEmpty(eventDetails.Category) || string.IsNullOrEmpty(eventDetails.Location) ||
                    eventDetails.StartTime == default(DateTime) || eventDetails.EndTime == default(DateTime) ||
                    eventDetails.RSVP_limit <= 0)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return RedirectToAction("CreateEvent");
                }

                var startTimeUtc = eventDetails.StartTime.ToUniversalTime();
                var endTimeUtc = eventDetails.EndTime.ToUniversalTime();

                // Create the event in the 'events' collection
                var eventData = new Dictionary<string, object>
        {
            { "EventName", eventDetails.EventName },
            { "Description", eventDetails.Description },
            { "Category", eventDetails.Category },
            { "Location", eventDetails.Location },
            { "StartTime", startTimeUtc }, 
            { "EndTime", endTimeUtc },      
            { "RSVP_limit", eventDetails.RSVP_limit },
            { "EventVisibility", eventDetails.EventVisibility }
        };

                DocumentReference eventRef = _firestoreDb.Collection("events").Document();
                await eventRef.SetAsync(eventData);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the event: " + ex.Message;
                return RedirectToAction("CreateEvent");
            }
        }

        // Method to save a festival
        [HttpPost]
        public async Task<IActionResult> SaveFestivalDetails(Festival festival)
        {
            try
            {
                festival.StartDate = festival.StartDate.ToUniversalTime();
                festival.EndDate = festival.EndDate.ToUniversalTime();

                // Query Firestore for ongoing festivals
                Query festivalQuery = _firestoreDb.Collection("festivals")
                    .WhereGreaterThanOrEqualTo("EndDate", Timestamp.FromDateTime(DateTime.UtcNow));

                QuerySnapshot querySnapshot = await festivalQuery.GetSnapshotAsync();

                if (querySnapshot.Documents.Count > 0)
                {
                    TempData["ErrorMessage"] = "A festival is already ongoing. You cannot create a new one.";
                    return RedirectToAction("AdminDashboard");
                }

                festival.Id = Guid.NewGuid().ToString();

                var festivalData = new Dictionary<string, object>
        {
            { "Id", festival.Id },
            { "FestivalName", festival.FestivalName },
            { "Location", festival.Location },
            { "StartDate", Timestamp.FromDateTime(festival.StartDate) },
            { "EndDate", Timestamp.FromDateTime(festival.EndDate) },
            { "Events", new List<string>() } 
        };

                // Save to Firestore
                DocumentReference docRef = _firestoreDb.Collection("festivals").Document(festival.Id);
                await docRef.SetAsync(festivalData);

                TempData["SuccessMessage"] = "Festival created successfully!";
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firestore Save Error");
                TempData["ErrorMessage"] = "An error occurred while creating the festival.";
                return RedirectToAction("CreateFestival");
            }
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }


        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var eventsCollection = _firestoreDb.Collection("events");
                var snapshot = await eventsCollection.GetSnapshotAsync();

                var eventsList = snapshot.Documents.Select(doc =>
                {
                    doc.TryGetValue("EventName", out string eventName);
                    doc.TryGetValue("Location", out string location);
                    doc.TryGetValue("StartTime", out Timestamp startTime);
                    doc.TryGetValue("EndTime", out Timestamp endTime);
                    doc.TryGetValue("RSVP_limit", out int attendeeLimit);

                    return new AdminEventsViewModel
                    {
                        Id = doc.Id,
                        Name = eventName ?? "N/A",
                        Location = location ?? "N/A",
                        StartTime = startTime.ToDateTime(),
                        EndTime = endTime.ToDateTime(),
                        AttendeeLimit = attendeeLimit
                    };
                }).ToList();

                return View(eventsList); // Pass data to the view
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading events: {ex.Message}";
                return RedirectToAction("AdminDashboard");
            }
        }

        public async Task<IActionResult> EventRsvps()
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
                return RedirectToAction("AdminDashboard");
            }
        }

        public IActionResult CreateEvent()
        {
            return View();
        }

        public IActionResult EditEvent()
        {
            return View();
        }

        public async Task<IActionResult> DeleteEvent()
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
                return RedirectToAction("AdminDashboard");
            }
        }

        // Method to delete an event
        [HttpPost]
        public async Task<IActionResult> deleteEvent(string eventName)
        {
            try
            {
                var eventsCollection = _firestoreDb.Collection("events");
                var query = eventsCollection.WhereEqualTo("EventName", eventName);
                var querySnapshot = await query.GetSnapshotAsync();

                if (!querySnapshot.Documents.Any())
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("DeleteEvent");
                }

                foreach (var document in querySnapshot.Documents)
                {
                    await document.Reference.DeleteAsync();
                }

                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the event: {ex.Message}";
            }

            return RedirectToAction("DeleteEvent");
        }


        public IActionResult CreateFestival()
        {
            return View();
        }
    }
}