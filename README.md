# One City Festival Management System

## Author Information
**Name:** Avish Judnarain  
**Course Code:** 4027W  
**Student Email:** jdnavi001@myuct.ac.za 
**Student Number:** JDNAVI001  

## Project Overview
The One City Festival Management System is a web-based platform developed using **C# .NET Framework**, **ASP.NET MVC**, **Firebase Authentication**, and **Firestore Cloudstore**. This system enables visitors to sign up for a festival, register for events, provide feedback, and allows administrators to manage the festival efficiently.

## Website Link
https://mini-project20250228010550.azurewebsites.net

## Features

### Visitor Features:
1. **User Registration & Authentication:** Sign up and log in using Firebase Authentication.
2. **Festival Registration:** Join the festival as a visitor.
3. **RSVP for Events:** Register for available festival events (subject to attendee limits).
4. **Rate Events:** Provide a rating (1 to 5 stars) for attended events.
5. **Recommend Events:** Rate whether they would recommend an event to others (1 to 5 stars).
6. **Leave Comments:** Share feedback and comments about attended events.

### Administrator Features:
1. **Manage Events:** Create, edit, delete, and archive events.
2. **View Visitor List:** See all registered visitors attending the festival.
3. **Manage Event Attendance:** View RSVPs for each event.
4. **Dashboard & Reports:**
   - **Admin Dashboard:** Displays key visitor demographics (e.g., age, gender) and event stats.
   - **Visitor Dashboard:** Displays event ratings, recommendations, comments for each event.

## Technologies Used
- **C# .NET Framework** (ASP.NET MVC) for backend development
- **Firebase Authentication** for user authentication
- **Firebase Firestore Cloudstore** for data storage and retrieval
- **Bootstrap & CSS** for responsive UI design

## Installation & Setup
1. **Clone the Repository:**
   ```sh
   git clone [repository-url]
   cd OneCityFestival
   ```
2. **Open the project in Visual Studio.**
3. **Install Dependencies:** Ensure that required NuGet packages are installed.
4. **Set Up Firebase:**
   - Create a Firebase project.
   - Enable Authentication (Email/Password sign-in method).
   - Configure Firestore database with required collections.
   - Update Firebase configuration in the project.
5. **Run the Application:**
   - Set the startup project.
   - Press `Ctrl + F5` to start the application.

## Database Structure (Firestore Collections)
- **Users** (Stores visitor and admin details)
- **Events** (Stores event details, categories, and limits)
- **RSVPs** (Stores visitor registrations for events)
- **Ratings** (Stores event ratings, recommendations)
- **Comments** (Stores event comments)  

## Future Enhancement possibilities that I see viable
- Implement a **waiting list** for full events.
- Add **email notifications** for event reminders.
- Integrate **real-time updates** for RSVP status.

## REFERENCES
C# Documentation (Microsoft)
https://learn.microsoft.com/en-us/dotnet/csharp/

Firebase Documentation (Official)
https://firebase.google.com/docs

ASP.NET MVC Overview (Microsoft)
https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/introduction-to-aspnet-mvc

Firebase SDK for .NET (Official)
https://firebase.google.com/docs/admin/setup#net

Introduction to MVC in C#
https://www.tutorialspoint.com/asp.net_mvc/asp.net_mvc_introduction.htm

How to Use Firebase with C#
https://medium.com/@kamalkd/Firebase-with-C-sharp-7c7cb139ebf1

Firebase Authentication with C# (TechNet)
https://techwhale.in/firebase-authentication-using-csharp/

Creating a Simple ASP.NET MVC Application (TutorialsPoint)
https://www.tutorialspoint.com/asp.net_mvc/asp.net_mvc_create_first_application.htm

C# Firebase Realtime Database Integration
https://csharp.hotexamples.com/examples/Firebase.Database/FirebaseDatabase/Set/csharp-firebase-database-set-method-examples.html

C# and Firebase Firestore Integration Tutorial
https://www.dev.to/rookies/firebase-firestore-integration-in-net-5-app-4f42

## License
This project is for academic purposes only.

---

Thank you for using the One City Festival Management System! :)

