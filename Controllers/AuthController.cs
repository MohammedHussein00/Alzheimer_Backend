using Alzaheimer.Models;
using Alzheimer.Models;
using Alzheimer.Services;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace Alzheimer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IAuthService _authService;
        public AuthController(IAuthService authService, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _authService = authService;
            _Context = context;
            _userManager = userManager;

        }

        [HttpPost("Register")]

        public async Task<IActionResult> RegisterAsync(RegisterModel model) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.RegisterAsync(model);
            if (!result.IsAuthenticated)
                return Ok(result);

            return Ok(result);
        }      
    
        
    [HttpPost("Register-doctor")]

    public async Task<IActionResult> RegisterDoctorAsync([FromForm]RegisterDoctor model) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.RegisterDoctorAsync(model);
            if (!result.IsAuthenticated)
                return Ok(result);

            return Ok(result);
        }   
        [HttpPost("Register-admin")]

    public async Task<IActionResult> RegisterAdminAsync(RegisterModel model) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.RegisterAdminAsync(model);
            if (!result.IsAuthenticated)
                return Ok(result);

            return Ok(result);
        }

        [HttpPost("login")]
        
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);
            if (!result.IsAuthenticated)
                return Ok(result);

            return Ok(result);
        }

        [HttpPost("AddRole")]

        public async Task<IActionResult> AddRoleAsync(AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.AddRoleAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);


            return Ok(model);
        }
        [HttpPost("update-doctor")]
        public async Task<IActionResult> Update([FromForm] UpdateDoctor d)
        {
            var updatedDoctor = await _authService.UpdateDoctor(d); // Ensure to await the asynchronous operation

            return Ok(updatedDoctor); // Return the updated doctor object directly
        }

        [HttpPost("update-pateint")]
        public async Task<IActionResult> UpdatePateint([FromForm] UpdateDoctor d)
        {

            var x = _authService.UpdateDoctor(d);
            return Ok(x);
        }




        [HttpPost("verify")]
        public async Task<IActionResult> SendEmailAsync(SendVerification model)
        {
            Random random = new Random();
            string code = "";
            for (int i = 0; i < 6; i++)
            {
                int digit = random.Next(0, 10); // Generates a random number between 0 and 9
                code+=digit.ToString();
                
            }

            if (_authService.SendEmail(model.UserName, model.Email, "verify your email", $"<h1>Hi {model.UserName} </h1><div>Please verify your email</div><div>Use next verification code to verify your email</div><a href=''>{code}</a>").IsCompletedSuccessfully)

                return Ok(new stringClass {data =code });
            else
                return Ok("there an error send again");
           
        }
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.EmailConfirmed = true;
                 _Context.Update(user);
                await _Context.SaveChangesAsync();
                return Ok(new stringClass { data = "Your email verified successuflly" });
            }
            else
            {
                return Ok(new stringClass { data = "There is a problem try again" });
            }
        }
    }
   

    
}
