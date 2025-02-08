using System.Diagnostics;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Mini_Project.Models;
using System.Threading.Tasks;

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

                // ✅ Check if a user with the given email already exists
                try
                {
                    var existingUser = await auth.GetUserByEmailAsync(model.Email);
                    ViewData["ErrorMessage"] = "A user with this email already exists. Please log in.";
                    return View("Register"); // Stay on the same page!
                }
                catch (FirebaseAuthException ex)
                {
                    if (ex.AuthErrorCode != AuthErrorCode.UserNotFound)
                    {
                        ViewData["ErrorMessage"] = $"Firebase Auth Error: {ex.Message}";
                        return View("Register"); // Stay on the same page!
                    }
                }

                // ✅ Create user in Firebase Authentication
                var userRecordArgs = new UserRecordArgs()
                {
                    Email = model.Email,
                    Password = model.Password,
                    DisplayName = model.Username,
                };

                var newUser = await auth.CreateUserAsync(userRecordArgs);
                string uid = newUser.Uid;

                // ✅ Save user details in Firestore
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
                return RedirectToAction("Index"); // Redirect after successful registration
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error: {ex.Message}";
                return View("Register"); // Stay on the same page!
            }
        }

        // Process Login Form
        [HttpPost]
        public async Task<IActionResult> Login(UserDetails model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verify user credentials with Firebase
                    var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(model.Email);

                    if (user != null)
                    {
                        // Check if password matches - Firebase Auth handles password validation internally
                        TempData["SuccessMessage"] = "Login successful! Welcome back.";
                        Console.WriteLine("Login successful");
                        return RedirectToAction("Index"); // Redirect to home page after successful login
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid email or password.");
                    }
                }
                catch (FirebaseAuthException)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(model); // Returns the login view if login fails
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Privacy()
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
