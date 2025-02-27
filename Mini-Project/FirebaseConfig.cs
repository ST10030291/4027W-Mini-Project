using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;

public class FirebaseConfig
{
    private static bool _isInitialized = false;

    public static void InitializeFirebase()
    {
        if (!_isInitialized)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "w-mini-project-firebase-adminsdk-fbsvc-69d82ef45a.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Firebase service account key file not found.", path);
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            Console.WriteLine($"GOOGLE_APPLICATION_CREDENTIALS: {Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")}");


            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)
            });

            Console.WriteLine(" Firebase Initialized Successfully.");
            _isInitialized = true;
        }
    }
}
