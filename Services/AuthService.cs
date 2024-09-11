using Alzaheimer.Models;
using Alzaheimer.Tables;
using Alzheimer.Helpers;
using Alzheimer.Models;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using SixLabors.ImageSharp;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Alzheimer.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly JWT _Jwt;
        private readonly IWebHostEnvironment _hostingEnvironment;



        public AuthService(IWebHostEnvironment hostingEnvironment, UserManager<IdentityUser> userManager
            , IOptions<JWT> jwt
            , RoleManager<IdentityRole> roleManager
            , ApplicationDbContext Context)
        {
            _Context = Context;
            _userManager = userManager;
            _Jwt = jwt.Value;
            _roleManager = roleManager;
            _hostingEnvironment = hostingEnvironment;

        }


        private Patient CreatePateint()
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
        private Doctor CreateDoctor()
        {
            try
            {
                return Activator.CreateInstance<Doctor>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Doctor)}'. " +
                    $"Ensure that '{nameof(Doctor)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private Admin CreateAdmin()
        {
            try
            {
                return Activator.CreateInstance<Admin>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Admin)}'. " +
                    $"Ensure that '{nameof(Admin)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }


        public async Task<AuthModel> RegisterAdminAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already Register" };

            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username is Already Exist" };

            var user = CreateAdmin();

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

            await _userManager.AddToRoleAsync(user, "Admin");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "Admin" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Name = user.Name,
                EmailConfirmed = user.EmailConfirmed




            };


        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already Register" };

            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username is Already Exist" };

            var user = CreatePateint();

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

            await _userManager.AddToRoleAsync(user, "Pateint");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "Pateint" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Name = user.Name,
                EmailConfirmed = user.EmailConfirmed
                



            };


        }



        public async Task<AuthModel> RegisterDoctorAsync(RegisterDoctor model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already Register" };

            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username is Already Exist" };

            var user = CreateDoctor();

            user.UserName = model.Email;
            user.Email = model.Email;

            user.Name = model.Name;
            user.PhoneNumber = model.Phone;
            user.DOB = model.DOB;
            user.Specialization = model.Specialization;
            string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DoctorData");
            Directory.CreateDirectory($"{folderPath}\\{model.Email}");
            var education = new Education();













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
            education.certification = $"{folderPath}\\{model.Email}\\{model.Certification.FileName}{Path.GetExtension(model.Certification.FileName)}";
            education.DoctorId = user.Id;
            education.University = "";
            education.Country = "";
            education.Year = 0;
            SaveFile(education.certification, model.Certification);

            _Context.Educations.Add(education);

            await _userManager.AddToRoleAsync(user, "Doctor");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "Doctor" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Name = user.Name,
                EmailConfirmed = user.EmailConfirmed




            };


        }

        public async Task<LoginResponse> GetTokenAsync(TokenRequestModel model)
        {
            var authmodel = new LoginResponse();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authmodel.Message = "Email or Password is Incorrect";
                return authmodel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            var pateint =await _Context.Patients.Where(e=>e.Email==user.Email).FirstOrDefaultAsync();
            var doctor = await _Context.Doctors.Where(e => e.Email == user.Email).FirstOrDefaultAsync();

            if (pateint is not null)
            {
                authmodel.Name = pateint.Name;
                authmodel.ImgUrl = pateint.ImgUrl;
            }

            else if (doctor is not null)
            {
                authmodel.Name = doctor.Name;
                authmodel.ImgUrl = doctor.ImgUrl;
            }
            authmodel.IsAuthenticated = true;
            authmodel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            
            authmodel.Email = user.Email;

            authmodel.ExpiredOn = jwtSecurityToken.ValidTo;


            var userRoles = await _userManager.GetRolesAsync(user);
            authmodel.Roles = userRoles.ToList();
            authmodel.EmailConfirmed = user.EmailConfirmed;
            return authmodel;
        }
        
        public async Task<UpdateDoctor> UpdateDoctor(UpdateDoctor doctor)
        {
            var user = await _Context.Doctors.FirstOrDefaultAsync(c => c.Email == doctor.Email);
            user.Name = doctor.Name;
            user.Email = doctor.Email;
            user.UserName = doctor.Email;
            user.DOB = doctor.DOB;
            user.PhoneNumber = doctor.Phone;
            user.SmallTip = doctor.SmallTip;
            user.AboutDoctor = doctor.AboutDoctor;
            user.Specialization = doctor.Specialization;
            user.NormalizedEmail = doctor.Email.ToUpper();
            user.NormalizedUserName = doctor.Email.ToUpper();

            if (doctor.Photo is not null)
            {
                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DoctorData");

                string floderName = $"{folderPath}\\{doctor.CurrentEmail}";
                //if (Directory.Exists(floderName))
                //{
                //    // Rename the directory
                //    Directory.Move(floderName, doctor.Email);
                //}
                if (File.Exists(user.ImgUrl))
                {
                    File.Delete(user.ImgUrl);
                }
                user.ImgUrl = $"{folderPath}\\{doctor.Email}\\{doctor.Photo.FileName}{Path.GetExtension(doctor.Photo.FileName)}";
                if (!File.Exists(user.ImgUrl))
                {
                    // Open a stream to write the file content
                    using (var stream = new FileStream(user.ImgUrl, FileMode.Create))
                    {
                        // Copy the file content to the stream asynchronously
                        await doctor.Photo.CopyToAsync(stream);
                    }
                }
                _Context.Doctors.Update(user);
            }
            await _Context.SaveChangesAsync();
            return doctor;
            
        }


        public async Task<UpdatePateint> UpdatePateint(UpdatePateint pateint)
        {
            var user = await _Context.Patients.FirstOrDefaultAsync(c => c.Email == pateint.Email);
            user.Name = pateint.Name;
            user.Email = pateint.Email;
            user.UserName = pateint.Email;
            user.DOB = pateint.DOB;
            user.PhoneNumber = pateint.Phone;
            user.NormalizedEmail = pateint.Email.ToUpper();
            user.NormalizedUserName = pateint.Email.ToUpper();

            if (pateint.Photo is not null)
            {
                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "UserData");

                string floderName = $"{folderPath}\\{pateint.CurrentEmail}";
                if (Directory.Exists(floderName))
                {
                    // Rename the directory
                    Directory.Move(floderName, pateint.Email);
                }
                else
                    Directory.CreateDirectory($"{folderPath}\\{pateint.Email}");

                if (File.Exists(user.ImgUrl))
                {
                    File.Delete(user.ImgUrl);
                }
                user.ImgUrl = $"{folderPath}\\{pateint.Email}\\{pateint.Photo.FileName}{Path.GetExtension(pateint.Photo.FileName)}";
                SaveFile($"{folderPath}\\{pateint.Email}\\{pateint.Photo.FileName}{Path.GetExtension(pateint.Photo.FileName)}", pateint.Photo);
                _Context.Patients.Update(user);
            }
            await _Context.SaveChangesAsync();
            return pateint;

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
        public static async void SaveFile(string path, IFormFile file)
        {
            // Check if the file and path are valid
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid file or path.");
            }

            // Create directory if it doesn't exist

            // Open a stream to write the file content
            using (var stream = new FileStream(path, FileMode.Create))
            {
                // Copy the file content to the stream asynchronously
                await file.CopyToAsync(stream);
            }
        }
        public async Task<bool> SendEmail(string userName,string userEmail, string subject, string textContent)
        {
            sib_api_v3_sdk.Client.Configuration.Default.ApiKey["api-key"] = "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";

            // Create an instance of the TransactionalEmailsApi
            var apiInstance = new TransactionalEmailsApi();

            // Define sender details
            string senderName = "Alzhiemer disease";
            string senderEmail = "mh2156@fayoum.edu.eg";
            SendSmtpEmailSender sender = new SendSmtpEmailSender(senderName, senderEmail);

            // Define recipient details
            string toEmail = userEmail;
            string toName = userName;
            SendSmtpEmailTo recipient = new SendSmtpEmailTo(toEmail, toName);
            List<SendSmtpEmailTo> recipients = new List<SendSmtpEmailTo>();
            recipients.Add(recipient);

            try
            {
                var sendSmtpEmail = new SendSmtpEmail(sender, recipients, null, null,textContent , "xdfxg", subject); // Pass an empty list for Bcc

                // Send the email
                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                return true;


            }
            catch (Exception e)
            {
                return false;
            }
        }


    }
}
