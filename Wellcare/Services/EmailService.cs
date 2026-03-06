using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace wellcare.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            string host = emailSettings["Host"];
            int port = int.Parse(emailSettings["Port"]);
            bool enableSsl = bool.Parse(emailSettings["EnableSsl"]);
            string userName = emailSettings["UserName"];
            string password = emailSettings["Password"];
            string from = emailSettings["From"];

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(userName, password);

                var message = new MailMessage();
                message.From = new MailAddress(from);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = "Your ElderPro Email Verification OTP";
                message.Body = $@"
Hi,

Your OTP for Wellnest is: {otp}

This OTP is valid for 10 minutes.

If you did not request this, you can safely ignore this email.

Regards,
Wellnest Team
";
                message.IsBodyHtml = false;

                await client.SendMailAsync(message);
            }
        }

        public async Task SendAlertEmailAsync(string toEmail, string elderName, string eventType, byte[] imageBytes)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string host = emailSettings["Host"];
            int port = int.Parse(emailSettings["Port"]);
            bool enableSsl = bool.Parse(emailSettings["EnableSsl"]);
            string userName = emailSettings["UserName"];
            string password = emailSettings["Password"];
            string from = emailSettings["From"];

            string subject = eventType == "FALL_DETECTED"
                ? $"FALL DETECTED - {elderName}"
                : $"IDLE ALERT - {elderName}";

            string description = eventType == "FALL_DETECTED"
                ? $"A fall has been detected for elder {elderName}. Please check on them immediately."
                : $"Elder {elderName} has been idle for an extended period. Please check on them.";

            using var client = new SmtpClient(host, port);
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(userName, password);

            var message = new MailMessage();
            message.From = new MailAddress(from);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: {(eventType == "FALL_DETECTED" ? "#EF4444" : "#F59E0B")}; padding: 20px; border-radius: 12px 12px 0 0;'>
                    <h2 style='color: white; margin: 0;'>{subject}</h2>
                </div>
                <div style='background: #F8FAFC; padding: 20px; border-radius: 0 0 12px 12px; border: 1px solid #E2E8F0;'>
                    <p style='font-size: 16px; color: #0F172A;'>{description}</p>
                    <p style='color: #64748B; font-size: 14px;'>Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                    <p style='color: #64748B; font-size: 14px;'>A snapshot has been attached to this email.</p>
                    <hr style='border: 1px solid #E2E8F0; margin: 20px 0;'/>
                    <p style='color: #94A3B8; font-size: 12px;'>Wellnest - Compassionate Care</p>
                </div>
            </div>";

            if (imageBytes != null)
            {
                var stream = new MemoryStream(imageBytes);
                message.Attachments.Add(new Attachment(stream, "snapshot.jpg", "image/jpeg"));
            }

            await client.SendMailAsync(message);
        }

        public async Task SendFollowUpEmailAsync(string toEmail, string elderName, string responseType)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string host = emailSettings["Host"];
            int port = int.Parse(emailSettings["Port"]);
            bool enableSsl = bool.Parse(emailSettings["EnableSsl"]);
            string userName = emailSettings["UserName"];
            string password = emailSettings["Password"];
            string from = emailSettings["From"];

            string subject = responseType switch
            {
                "OKAY" => $"{elderName} is okay",
                "NOT_OKAY" => $"{elderName} needs help!",
                "NO_RESPONSE" => $"{elderName} did not respond",
                _ => $"Alert update for {elderName}"
            };

            string bodyText = responseType switch
            {
                "OKAY" => $"{elderName} responded and confirmed they are okay.",
                "NOT_OKAY" => $"{elderName} responded and indicated they need help. Please check immediately!",
                "NO_RESPONSE" => $"{elderName} did not respond within 60 seconds. Please check on them immediately!",
                _ => ""
            };

            string color = responseType switch
            {
                "OKAY" => "#10B981",
                "NOT_OKAY" => "#EF4444",
                "NO_RESPONSE" => "#F59E0B",
                _ => "#64748B"
            };

            using var client = new SmtpClient(host, port);
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(userName, password);

            var message = new MailMessage();
            message.From = new MailAddress(from);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = $@"
        <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: {color}; padding: 20px; border-radius: 12px 12px 0 0;'>
                <h2 style='color: white; margin: 0;'>{subject}</h2>
            </div>
            <div style='background: #F8FAFC; padding: 20px; border-radius: 0 0 12px 12px; border: 1px solid #E2E8F0;'>
                <p style='font-size: 16px; color: #0F172A;'>{bodyText}</p>
                <p style='color: #64748B; font-size: 14px;'>Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                <hr style='border: 1px solid #E2E8F0; margin: 20px 0;'/>
                <p style='color: #94A3B8; font-size: 12px;'>Wellnest - Compassionate Care</p>
            </div>
        </div>";

            await client.SendMailAsync(message);
        }
    }
}