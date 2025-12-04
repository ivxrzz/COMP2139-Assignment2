using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Assignment1.Services
{
    // Very simple email sender that does nothing,
    // but satisfies Identity's requirement.
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Do nothing — BASIC, SIMPLE, SAFE.
            return Task.CompletedTask;
        }
    }
}