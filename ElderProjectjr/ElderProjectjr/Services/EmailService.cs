using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ElderProjectjr.Services
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
    }
}
