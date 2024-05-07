using Humanizer;
using MailKit.Security;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using TTTN3.Interfaces;
using TTTN3.Models;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace TTTN3.Services
{
    public class EmailSender : IMailSender
    {

        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail)); // Empty display name
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient()) // Specify the full namespace
            {
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.SmtpUsername, emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        }

}