using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace TwitterCopy.Core.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        // Set only via Secret Manager
        public AuthMessageSenderOptions Options { get; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(Options.SendGridKey, subject, htmlMessage, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("therousage@gmail.com", name: "RouSage"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }
    }
}
