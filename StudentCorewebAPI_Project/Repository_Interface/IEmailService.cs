using System.Threading.Tasks;
using StudentCorewebAPI_Project.Enums;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
