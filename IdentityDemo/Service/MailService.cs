using System.Net.Mail;
using System.Net;

namespace IdentityDemo.Service
{

    public interface IMailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class MailService : IMailService
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //Email verification
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("jeelsavaliya007@gmail.com");
            message.To.Add(email);
            message.Subject = "Confirm your email";
            message.IsBodyHtml = true;
            message.Body = htmlMessage;

            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com"; //for gmail host

            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("jeelsavaliya007@gmail.com", "jkqw rnhk kdfv crwh"); //password is create from gmail security
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
        }
    }

}
