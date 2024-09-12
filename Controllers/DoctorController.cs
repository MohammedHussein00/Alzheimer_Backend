using Alzaheimer.Models;
using Alzaheimer.Services;
using Alzheimer.Models;
using Alzheimer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Tensorflow;
using NumSharp;
using Alzheimer.Tables;
using Alzaheimer.Tables;
using Google.Apis.Drive.v3.Data;
using Tensorflow.Eager;

namespace Alzaheimer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        private readonly IWebHostEnvironment _hostingEnvironment;


        private readonly IDoctorService _doctorService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserService _userService;

        public DoctorController(IUserService userService,IWebHostEnvironment hostingEnvironment, IDoctorService doctorService, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {

            _doctorService = doctorService;
            _Context = context;
            _userManager = userManager;
            _userService = userService;

            _hostingEnvironment = hostingEnvironment;

        }

        [HttpPost("get-clinices")]
        public async Task<IActionResult> GetClinicsAsync(DoctorReturn doc)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(doc.Email); // Await the task to get the actual user

                if (user is not null)
                {
                    // Retrieve specific columns from the Clinics table
                    var clinics = await _Context.Clinics
                        .Where(d => d.DoctorId == user.Id)
                        .Select(c => new ClinicGet
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Discription,
                            Examination = c.Examination,
                            Phone = c.Phone,
                            Location = c.Location,
                            Assist = _Context.Assistants
                                .Where(a => a.ClinicId == c.Id)
                                .Select(i => new Assist { Phone = i.Number, Name = i.Name, Id = i.Id })
                                .ToList(),
                            Images = _Context.ClinicImages
                                .Where(i => i.ClinicId == c.Id)
                                .Select(i => new ImageDto { Id = i.Id, ImageUrl = i.ImageUrl }) // Select just the ImageUrl property
                                .ToList()
                        })
                        .ToListAsync();

                    return Ok(clinics);
                }
                else
                {
                    // Return an empty array if user is not found
                    return Ok(new ClinicGet[0]);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }






        [HttpPost("update-clinic")]
        public async Task<IActionResult> UpdateClinicAsync([FromForm] ClinicDto model)
        {


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _doctorService.UpdateClinicAsync(model);

            return Ok(result);

        }
        [HttpPost("doc-uodate")]
        public async Task<IActionResult> GetDoctorAsync(DoctorReturn doc)
        {


            try
            {
                // Retrieve specific columns from the Doctors table
                var doctors = await _Context.Doctors.Where(d => d.Email == doc.Email)
                    .Select(d => new
                    {
                        d.Email,
                        d.Name,
                        d.PhoneNumber,
                        d.AboutDoctor,
                        d.DOB,
                        d.SmallTip,
                        d.Specialization,
                        d.ImgUrl


                    }) // Adjust these properties as needed
                    .FirstOrDefaultAsync();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update-shedule")]
        public async Task<IActionResult> UpdateClinicAsync(ScheduleDto model)
        {


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _doctorService.UpdateSchedulAsync(model);

            return Ok(result);

        }
        [HttpPost("get-schedule")]
        public async Task<IActionResult> GetScheduleAsync(DoctorReturn email)
        {
            try
            {
                if (string.IsNullOrEmpty(email.Email))
                {
                    return BadRequest("Email cannot be empty.");
                }

                var user = await _Context.Doctors.FirstOrDefaultAsync(u => u.Email == email.Email);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var schedules = await _Context.Schedules
                    .Where(s => s.DoctorId == user.Id)
                    .Select(s => new
                    {
                        s.Id,
                        s.NextWeek,
                        s.ThisWeek,
                        s.clinic.Name,
                        s.ClinicId,
                        s.NextTwoWeek,
                        s.NextTreeWeek,
                        Shifts = _Context.scheduleShifts
                            .Where(ss => ss.ScheduleId == s.Id)
                            .Select(ss => new
                            {
                                ShiftDetails = _Context.Shifts.Select(sh => new { sh.Id, sh.Day, sh.EndTime, sh.StartTime }).FirstOrDefault(sh => sh.Id == ss.ShiftId)
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving schedule: {ex.Message}");
            }
        }

        [HttpPost("detect")]
        public async Task<IActionResult> DetectAsync([FromForm] DetectionDto model)
        {
            var patient = await _Context.Patients.Where(p => p.Email == model.PatientEmail).FirstOrDefaultAsync();

            var doctor = await _Context.Doctors.Where(d => d.Email == model.DoctorEmail).FirstOrDefaultAsync();
            if (doctor is not null)
            {
                var detection = new Detection();
                if (doctor is not null)
                    detection.DoctorId = doctor.Id;
                if (patient is not null)
                    detection.PatientId = patient.Id;
                detection.Date = DateTime.Now;
                _Context.Add(detection);
                _Context.SaveChanges();
                DateTime now = DateTime.Now;
                DateTime nowTruncated = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                // Query the database for detections matching the current date and time up to the minute
                var lastDetection = await _Context.Detections
                        .OrderByDescending(c => c.Id) // Assuming 'Id' is an auto-increment primary key
                        .FirstOrDefaultAsync();
                foreach (var detectionResult in model.DetectionResult)
                {
                    string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DoctorData");
                    if (!Directory.Exists($"{folderPath}\\{model.DoctorEmail}\\Detection"))
                        Directory.CreateDirectory($"{folderPath}\\{model.DoctorEmail}\\Detection");

                    var i = await _Context.DetectionMRIs.Where(a => a.MRIUrl == $"{folderPath}\\{model.DoctorEmail}\\Detection\\{detectionResult.File.FileName}{Path.GetExtension(detectionResult.File.FileName)}").FirstOrDefaultAsync();
                    if (i is null)
                    {
                        var detectionMRI = new DetectionMRI();

                        detectionMRI.MRIUrl = $"{folderPath}\\{model.DoctorEmail}\\Detection\\{detectionResult.File.FileName}{Path.GetExtension(detectionResult.File.FileName)}";
                        detectionMRI.DetectionId = lastDetection.Id;
                        detectionMRI.Result = detectionResult.Result;
                        _Context.DetectionMRIs.Add(detectionMRI);
                        _Context.SaveChanges();
                        if (!System.IO.File.Exists(detectionMRI.MRIUrl))
                        {
                            // Open a stream to write the file content
                            using (var stream = new FileStream(detectionMRI.MRIUrl, FileMode.Create))
                            {
                                // Copy the file content to the stream asynchronously
                                await detectionResult.File.CopyToAsync(stream);
                            }
                        }




                    }

                }
            }

            return Ok(model);
        }
        [HttpGet("get-appointment-doctor")]

        public async Task<IActionResult> GetApointmentAsync(string email)
        {
            try
            {
                var doctor = await _Context.Doctors.Where(p => p.Email == email).FirstOrDefaultAsync();
                if (doctor != null)
                {
                    var appointments = await _Context.Appointments
                        .Where(a => a.DoctorId == doctor.Id)
                        .Select(a => new
                        {
                            a.Id,
                            a.AppDay,
                            a.ShiftId,
                            a.BookingToOther,
                            a.PatientPhone,
                            a.PatientName,
                            a.PatientEmail,
                            a.PatientId,
                            PatientImg = _Context.Patients
                                .Where(d => d.Id == a.PatientId)
                                .Select(d => d.ImgUrl)
                                .FirstOrDefault(),


                            DoctorName = _Context.Doctors
                                .Where(d => d.Id == a.DoctorId)
                                .Select(d => d.Name)
                                .FirstOrDefault(),
                            AppointmentStatus = _Context.AppointmentStatuses
                                .Where(s => s.Id == a.appointmentStatusId)
                                .Select(s => s.Status)
                                .FirstOrDefault(),

                            Location = _Context.Shifts
                                .Where(s => s.Id == a.ShiftId)
                                .Select(s => _Context.scheduleShifts
                                    .Where(sh => sh.ShiftId == s.Id)
                                    .Select(sh => sh.ScheduleId)
                                    .FirstOrDefault())
                                .Select(sh => _Context.Schedules
                                    .Where(sch => sch.Id == sh)
                                    .Select(sch => _Context.Clinics
                                        .Where(c => c.Id == sch.ClinicId)
                                        .Select(c => c.Location)
                                        .FirstOrDefault())
                                    .FirstOrDefault()
                                    )
                                .FirstOrDefault()
                        })
                        .ToListAsync();

                    return Ok(appointments);
                }
                else
                {
                    return Ok("");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }


        [HttpGet("get-doctor-by-email")]
        public async Task<IActionResult> GetDoctorByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email cannot be empty");

            try
            {
                // Retrieve specific columns from the Doctors table along with related data
                var doctor = await _Context.Doctors
                    .Where(d => d.Email == email)
                    .Select(d => new
                    {
                        d.Email,
                        //d.Id,
                        d.Name,

                        d.ImgUrl,
                        Clinics = d.Clinics
                            .Where(c => c.DoctorId == d.Id)
                            .Select(c => new
                            {
                                c.Id,
                                c.Examination,
                                c.Name

                            })
                            .ToList(),

                        Schedule = d.Schedules.Where(s => s.DoctorId == d.Id)
                       .Select(s => new
                       {

                           s.ClinicId,
                           SatShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Sat" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime
                        })
                        .ToList(),
                           SunShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Sun" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList(),
                           MunShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Mun" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList(),
                           TueShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Tue" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList(),
                           WedShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Wed" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList(),
                           ThuShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Thu" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList(),
                           FriShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Fri" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            //ss.Id,
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime

                        })
                        .ToList()
                       })
                    })
                    .FirstOrDefaultAsync();


                if (doctor == null)
                    return NotFound("Doctor not found");

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("get-detection-by-email")]
        public async Task<IActionResult> GetAllDetectionByEmailAsync(string email)
        {
            try
            {
                _Context.Database.SetCommandTimeout(180);

                var doctor = await _Context.Doctors.FirstOrDefaultAsync(d => d.Email == email);

                if (doctor == null)
                    return NotFound("Doctor not found");

                var detections = await _Context.Detections
                    .Include(d => d.DetectionMRIs)
                    .Where(d => d.DoctorId == doctor.Id)
                    .Select(det => new
                    {
                        det.Id,
                        det.Date,
                        det.PatientId,
                        DetectionMRIs = det.DetectionMRIs.Select(dm => new
                        {
                            dm.Id,
                            dm.MRIUrl,
                            dm.Result
                        }).ToList()
                    }).ToListAsync();

                return Ok(detections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("send-message")]

        public async Task<IActionResult> SendMessageFromDoctorToPatient(Message message)
        {
            try
            {
                var doctor = _Context.Doctors.Where(patient => patient.Email == message.SenderId).FirstOrDefault();
                var chat = new Chat
                {
                    Content = message.Content,
                    Time = DateTime.Now,
                    SenderId = doctor.Id,
                    SenderType = Tables.SenderType.Doctor,
                    ReceiverId = message.ReceiverId,
                    Status = false
                };

                _Context.Chats.Add(chat);
                _Context.SaveChanges();
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
        [HttpPost("seen-messages")]

        public async Task<IActionResult> SendMessageFromDoctorToPatient(Message[] messages)
        {
            try
            {
                foreach (var chat in messages)
                {
                    var c = _Context.Chats.Where(c => c.Id == chat.Id).FirstOrDefault();
                    if (c is not null)
                    {
                        c.Status = true;
                        _Context.Update(c);
                        _Context.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
        [HttpPost("send-message-to-admin")]

        public async Task<IActionResult> SendMessageFromDoctorToAdmin(Message message)
        {
            try
            {
                var doctor = _Context.Doctors.Where(patient => patient.Email == message.SenderId).FirstOrDefault();
                var chat = new Chat
                {
                    Content = message.Content,
                    Time = DateTime.Now,
                    SenderId = doctor.Id,
                    SenderType = Tables.SenderType.Doctor,
                    ReceiverId = message.ReceiverId,
                    Status = false
                };

                _Context.Chats.Add(chat);
                _Context.SaveChanges();
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
        [HttpGet("get-chat")]
        public async Task<IActionResult> GetChatByEmailAsync(string email)
        {
            try
            {
                var patient = _Context.Doctors.Where(a => a.Email == email).FirstOrDefault();

                if (patient is not null)
                {
                    var messages = _Context.Chats
                   .Where(m => m.SenderId == patient.Id || m.ReceiverId == patient.Id).Select(c => new
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


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }
        }
        [HttpPost("cancel-all")]
        public async Task<IActionResult> CancelAllAppointmentsAsync(AppointmentDto[] apps)
        {
            try
            {
                foreach (var app in apps)
                {


                    var a = _Context.Appointments.Where(patient => patient.Id == app.Id).FirstOrDefault();
                    if (a is not null)
                    {
                        a.appointmentStatusId = 3;
                        _Context.Update(a);
                        _Context.SaveChanges();
                        var doctor = _Context.Doctors.Where(d => d.Id == a.DoctorId).FirstOrDefault();

                        var shift = _Context.Shifts.Where(s => s.Id == a.ShiftId).FirstOrDefault();

                        if (_userService.SendEmail(a.PatientName, a.PatientEmail, "Cancel Appointment", $"<h1>Hi  {a.PatientName} </h1><div>Dr: {doctor.Name} just Cancel an appointment</div><div> Day : {a.AppDay} At Shift From {shift.StartTime} to {shift.EndTime}</div>").IsCompletedSuccessfully) ;

                    }
                }
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }

        }
        [HttpPost("confirm-complete-appointment")]
        public async Task<IActionResult> Confirm(AppointmentDto[] apps)
        {
            try
            {
                foreach (var app in apps)
                {

                    var a = _Context.Appointments.Where(patient => patient.Id == app.Id).FirstOrDefault();
                    if (a is not null)
                    {
                        a.appointmentStatusId = 3;
                        _Context.Update(a);
                        _Context.SaveChanges();
                    }
                }
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");

            }

        }
    }
}
