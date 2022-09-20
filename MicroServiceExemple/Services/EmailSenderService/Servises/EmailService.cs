using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net;

namespace EmailSenderService.Servises
{
    public interface IEmailService
    {
        Task SendEmailAsync(ICollection<string> emails, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfig ec;

        public EmailService(IOptions<EmailConfig> emailConfig)
        {
            this.ec = emailConfig.Value;
        }

        public async Task SendEmailAsync(ICollection<string> emails, string subject, string message)
        {
            ArgumentNullException.ThrowIfNull(nameof(emails));
            ArgumentNullException.ThrowIfNull(nameof(subject));
            ArgumentNullException.ThrowIfNull(nameof(message));

            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(ec.FromName, ec.FromAddress));
                foreach(var email in emails) emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(TextFormat.Text) { Text = message };

                using (var client = new SmtpClient())
                {
                    //client.LocalDomain = ec.LocalDomain;

                    await client.ConnectAsync(ec.MailServerAddress, Convert.ToInt32(ec.MailServerPort), SecureSocketOptions.Auto).ConfigureAwait(false);
                    await client.AuthenticateAsync(new NetworkCredential(ec.UserId, ec.UserPassword));
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
