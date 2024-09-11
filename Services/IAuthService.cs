using Alzaheimer.Models;
using Alzheimer.Models;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Alzheimer.Services
{
    public interface IAuthService
    {

        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> RegisterAdminAsync(RegisterModel model);
        Task<AuthModel> RegisterDoctorAsync(RegisterDoctor model);

        Task<LoginResponse> GetTokenAsync(TokenRequestModel model);

        Task<string> AddRoleAsync(AddRoleModel model);
        Task<UpdateDoctor> UpdateDoctor(UpdateDoctor doctor);
        Task<bool> SendEmail(string userName, string userEmail, string subject, string textContent);
    }

}
