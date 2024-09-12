using Alzaheimer.Models;
using Alzaheimer.Services;
using Alzaheimer.Tables;
using Alzheimer.Models;
using Alzheimer.Services;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Tensorflow.Eager;

namespace Alzaheimer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly ApplicationDbContext _Context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IAuthService _authService;
        public AdminController(IAuthService authService, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _authService = authService;
            _Context = context;
            _userManager = userManager;

        }
        [HttpGet("get-doctors")]
        public async Task<IActionResult> GetDoctorsAsync()
        {
            try
            {
                var doctors = await _Context.Doctors
                    .Include(d => d.Clinics)
                    .Select(d => new
                    {
                        d.Id,
                        d.Email,
                        d.Name,
                        d.Specialization,
                        d.SmallTip,
                        d.Rate1,
                        d.Rate2,
                        d.Rate3,
                        d.Rate4,
                        d.Rate5,
                        d.PhoneNumber,
                        d.PhoneNumberConfirmed,
                        Certification = d.Educations
                            .Where(e => e.DoctorId == d.Id && e.certification != null) // Ensure certification is not null
                            .Select(e => e.certification)
                            .FirstOrDefault(), // Fetch the first certification or null
                        d.ImgUrl,
                        Clinics = d.Clinics.Select(c => new { c.Examination, c.Location }).ToList()
                    })
                    .ToListAsync();

                var result = doctors.Select(d =>
                new
                {
                    d.Id,
                    d.Email,
                    d.Name,
                    d.Specialization,
                    d.SmallTip,
                    d.Rate1,
                    d.Rate2,
                    d.Rate3,
                    d.Rate4,
                    d.Rate5,
                    d.PhoneNumber,
                    d.PhoneNumberConfirmed,
                    d.ImgUrl,
                    Examination = d.Clinics.FirstOrDefault()?.Examination,
                    Location = d.Clinics.FirstOrDefault()?.Location,
                    Certification = d.Certification ?? "" // Ensure Certification is not null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpDelete("delete-doctor")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var doctor = await _Context.Doctors.Where(d => d.Id == id).FirstOrDefaultAsync();

            if (doctor == null)
            {
                return NotFound();
            }

            _Context.Doctors.Remove(doctor);
            await _Context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage(Message message)
        {
            try
            {
                // Retrieve admin's ID based on the email sent from Angular
                var admin = await _Context.Admins.FirstOrDefaultAsync(a => a.Email == message.SenderId);

                string id = "";
                if (admin is not null)
                    id = admin.Id;
                var doctor = await _Context.Doctors.FirstOrDefaultAsync(a => a.Email == message.SenderId);
                if (doctor is not null)
                    id = doctor.Id;
                var patient = await _Context.Patients.FirstOrDefaultAsync(a => a.Email == message.SenderId);
                if (patient is not null)
                    id = patient.Id;


                // Create a new Chat object
                var chat = new Chat
                {
                    Content = message.Content,
                    Time = DateTime.Now,
                    SenderId = id, // Assign admin's ID as sender ID
                    SenderType = Tables.SenderType.Admin, // Set sender type as Admin
                    ReceiverId = message.ReceiverId, // Set receiver ID (Doctor ID from Angular)
                    Status = false // Initially set status to false (unread)
                };

                // Add chat to context and save changes
                _Context.Chats.Add(chat);
                await _Context.SaveChangesAsync();

                // Return the created chat object
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpGet("accept")]

        public async Task<IActionResult> Accepetdoctor(string id)
        {
            try
            {
                var doctor = await _Context.Doctors.Where(d => d.Id == id).FirstOrDefaultAsync();
                doctor.PhoneNumberConfirmed = true;
                _Context.Update(doctor);
                _Context.SaveChanges();


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
        [HttpGet("admins-and-doctors")]
        public async Task<IActionResult> GetMessagesFromAdminsAndDoctors(string senderEmail, string receiverId)
        {
            var admin = _Context.Admins.Where(a => a.Email == senderEmail).FirstOrDefault();
            var doctor = _Context.Doctors.Where(a => a.Email == senderEmail).FirstOrDefault();

            if (admin is not null)
            {
                var messages = _Context.Chats
               .Where(m => m.SenderId == admin.Id && m.ReceiverId == receiverId || m.SenderId == receiverId && m.ReceiverId == admin.Id).Select(c => new
               {
                   c.Content,
                   c.Id,
                   c.ReceiverId,
                   c.SenderId,
                   c.SenderType,
                   c.Time
               })
               .ToList();
                return Ok(messages);

            }
            else if (doctor is not null)
            {
                var messages = _Context.Chats
            .Where(m => m.SenderId == doctor.Id && m.ReceiverId == receiverId || m.SenderId == receiverId && m.ReceiverId == doctor.Id).Select(c => new
            {
                c.Content,
                c.Id,
                c.ReceiverId,
                c.SenderId,
                c.SenderType,
                c.Time
            })
            .ToList();
                return Ok(messages);

            }
            else
            {
                var pateint = _Context.Patients.Where(a => a.Email == senderEmail).FirstOrDefault();
                var messages = _Context.Chats
                .Where(m => m.SenderId == pateint.Id && m.ReceiverId == receiverId || m.SenderId == receiverId && m.ReceiverId == pateint.Id).Select(c => new
                {
                    c.Content,
                    c.Id,
                    c.ReceiverId,
                    c.SenderId,
                    c.SenderType,
                    c.Time
                })
                .ToList();
                return Ok(messages);


            }
        }
        [HttpGet("get-chat")]
        public async Task<IActionResult> GetChatByEmailAsync(string email)
        {
            try
            {
                var admin = _Context.Admins.Where(patient => patient.Email == email).FirstOrDefault();


                if (admin is not null)
                {
                    var message = _Context.Chats.Where(c => c.ReceiverId == admin.Id).Select(
                        c => new
                        {
                            c.Id,
                            c.Content,
                            c.Time,
                            c.Status,
                            docotor = _Context.Doctors.Where(p => p.Id == c.SenderId).Select(
                                p => new
                                {
                                    p.Name,
                                    p.ImgUrl
                                }).FirstOrDefault()
                        }).ToList();
                    return Ok(message);
                }


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
    }


}
