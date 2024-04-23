using Alzheimer.Helpers;
using Alzheimer.Models;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Alzheimer.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly JWT _Jwt;


        public AuthService(UserManager<IdentityUser> userManager
            , IOptions<JWT> jwt
            , RoleManager<IdentityRole> roleManager
            , ApplicationDbContext Context)
        {
            _Context = Context;
            _userManager = userManager;
            _Jwt = jwt.Value;
            _roleManager = roleManager;
        }


        private Patient CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Patient>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Patient)}'. " +
                    $"Ensure that '{nameof(Patient)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }



        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already Register" };

            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username is Already Exist" };

            var user = CreateUser();

            user.UserName = model.Email;
            user.Email = model.Email;

            user.Name = model.Name;
            user.PhoneNumber = model.Phone;
            user.DOB = model.DOB;





            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,


            };


        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authmodel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authmodel.Message = "Email or Password is Incorrect";
                return authmodel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);

            authmodel.IsAuthenticated = true;
            authmodel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authmodel.Username = user.UserName;
            
            authmodel.Email = user.Email;
            authmodel.ExpiredOn = jwtSecurityToken.ValidTo;


            var userRoles = await _userManager.GetRolesAsync(user);
            authmodel.Roles = userRoles.ToList();
            return authmodel;
        }
        
        public async Task<Patient> EditAsync(Patient customer)
        {
            var user = await _Context.Patients.FirstOrDefaultAsync(c => c.Id == customer.Id);
            user.Name = customer.Name;
            await _Context.SaveChangesAsync();
            return user;
            
        }

            public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user =await _userManager.FindByIdAsync(model.UserId);
            if (user == null || !await _roleManager.RoleExistsAsync(model.Role) )
                return "Invalid User Id or Role";


            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User Already Exist in this Role";
            
            var result=await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
                return string.Empty;

            return "Something went Wrong";
          
        }




        private async Task<JwtSecurityToken> CreateJwtToken(IdentityUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("role", role));

            var claims = new[]
            {
                  new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                  new Claim(JwtRegisteredClaimNames.Email, user.Email),
                  new Claim("uid",user.Id),

            }.Union(userClaims).Union(roleClaims);

            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Jwt.Key));

            var signingCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256);


            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _Jwt.ValidIssuer,
                audience: _Jwt.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(_Jwt.DurationInDays),
                signingCredentials: signingCredentials
                );

            return jwtSecurityToken;
        }


    }
}
