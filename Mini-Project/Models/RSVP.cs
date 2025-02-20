using Google.Cloud.Firestore;

[FirestoreData]
public class RSVP
{
    [FirestoreProperty]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty]
    public string EventName { get; set; }

    [FirestoreProperty]
    public string UserId { get; set; }

    [FirestoreProperty]
    public string PackageChoice { get; set; }  // None, Basic, Drinks, Food
}
