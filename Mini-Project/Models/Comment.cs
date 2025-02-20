using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    [FirestoreData]
    public class Comment
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string EventId { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }
    }
}
