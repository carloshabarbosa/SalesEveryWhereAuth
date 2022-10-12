using Google.Cloud.Firestore;

namespace Api.Model
{
    [FirestoreData]
    public class UserFirebase
    {
        [FirestoreProperty("userName")]
        public string UserName { get; set; }

        [FirestoreProperty("password")]
        public string Password { get; set; }

        [FirestoreProperty("businessUnit")]
        public string BusinessUnit { get; set; }
    }
}