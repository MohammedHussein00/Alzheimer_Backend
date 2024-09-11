using Alzaheimer.Models;

namespace Alzaheimer.Services
{
    public interface IUserService
    {
        Task<string> AddReviewAsync(ReviewDto model);
        Task<string> AddAppointmentAsync(AppointmentDto model);
        Task<bool> SendEmail(string userName, string userEmail, string subject, string textContent);
    }
}
