using System.Diagnostics;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Mini_Project.Models;
using System.Threading.Tasks;
using RestSharp;

namespace Mini_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            FirebaseConfig.InitializeFirebase();
            _firestoreDb = FirestoreDb.Create("w-mini-project");
            _logger = logger;
        }

        // Register User Method 
        [HttpPost]
        public async Task<IActionResult> SaveUserDetails(UserDetails model)
        {
            try
            {
                var auth = FirebaseAuth.DefaultInstance;

                // Check if a user with the given email already exists
                try
                {
                    var existingUser = await auth.GetUserByEmailAsync(model.Email);
                    ViewData["ErrorMessage"] = "A user with this email already exists. Please log in.";
                    return View("Register"); 
                }
                catch (FirebaseAuthException ex)
                {
                    if (ex.AuthErrorCode != AuthErrorCode.UserNotFound)
                    {
                        ViewData["ErrorMessage"] = $"Firebase Auth Error: {ex.Message}";
                        return View("Register"); 
                    }
                }

                // Create user in Firebase Auth
                var userRecordArgs = new UserRecordArgs()
                {
                    Email = model.Email,
                    Password = model.Password,
                    DisplayName = model.Username,
                };

                var newUser = await auth.CreateUserAsync(userRecordArgs);
                string uid = newUser.Uid;

                // Save user details in Firestore
                CollectionReference usersCollection = _firestoreDb.Collection("Users");
                var userData = new Dictionary<string, object>
        {
            { "UID", uid },
            { "Email", model.Email },
            { "Username", model.Username },
            { "Role", "Visitor" },
            { "Age", model.Age },
            { "Gender", model.Gender },
            { "Province", model.Province },
            { "FestivalAttendanceAmt", model.FestivalAttendanceAmt },
            { "DateCreated", DateTime.UtcNow }
        };

                await usersCollection.Document(uid).SetAsync(userData);

                TempData["SuccessMessage"] = "User registered successfully!";
                if (model.Role.ToLower() == "admin")
                {
                    return RedirectToAction("AdminDashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("VisitorDashboard", "Visitor");
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error: {ex.Message}";
                return View("Register");
            }
        }

        // Login User
        [HttpPost] 
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login");
            }

            try
            {
                var auth = FirebaseAuth.DefaultInstance;

                // Check if the email exists in Firebase Auth
                UserRecord userRecord;
                try
                {
                    userRecord = await auth.GetUserByEmailAsync(model.Email);
                }
                catch (FirebaseAuthException)
                {
                    ViewData["ErrorMessage"] = "Invalid email or password.";
                    return View("Login");
                }

                // Verify password using Firebase REST API
                string apiKey = "AIzaSyCSXynlYk7r4OS5dD7PmsUgYCzi2urRr5s";
                string firebaseAuthUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

                var client = new RestClient(firebaseAuthUrl);
                var request = new RestRequest("", Method.Post);
                request.AddJsonBody(new
                {
                    email = model.Email,
                    password = model.Password,
                    returnSecureToken = true
                });

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    ViewData["ErrorMessage"] = "Invalid email or password.";
                    return View("Login");
                }

                // Retrieve user role from Firestore based on email
                var userQuery = _firestoreDb.Collection("Users").WhereEqualTo("Email", model.Email);
                var userSnapshot = await userQuery.GetSnapshotAsync();

                if (userSnapshot.Documents.Count == 0)
                {
                    ViewData["ErrorMessage"] = "User not found in Firestore.";
                    return View("Login");
                }

                var userDoc = userSnapshot.Documents[0]; 
                string role = userDoc.GetValue<string>("Role");

                // Redirect based on role
                if (role == "Admin")
                {
                    TempData["SuccessMessage"] = "Welcome, Admin!";
                    return RedirectToAction("AdminDashboard", "Admin");
                }
                else if (role == "Visitor")
                {
                    TempData["SuccessMessage"] = "Welcome, Visitor!";
                    return RedirectToAction("VisitorDashboard", "Visitor");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Invalid role assigned.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error: {ex.Message}";
                return View("Login");
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            TempData["SuccessMessage"] = null;
            return View();
        }

        public IActionResult Login()
        {
            TempData["SuccessMessage"] = null;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
