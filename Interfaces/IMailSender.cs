    using System.Threading.Tasks;
namespace TTTN3.Interfaces
{
    // IMailService.cs

    public interface IMailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

}
