using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace wellcare.Services
{
    public class FcmService
    {
        public FcmService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("Firebase/firebase-adminsdk.json")
                });
            }
        }

        public async Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string> data = null)
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"FCM notification sent: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FCM send failed: {ex.Message}");
            }
        }
    }
}