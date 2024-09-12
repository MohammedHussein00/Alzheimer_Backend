using Alzaheimer.Models;
using Alzaheimer.Services;
using Alzaheimer.Tables;
using Alzheimer.Models;
using Alzheimer.Services;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Tensorflow.Eager;

namespace Alzaheimer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        private readonly IUserService _userService;
        private readonly ILogger<UserServiceController> _logger;

        public UserServiceController(IUserService userService, ApplicationDbContext context, ILogger<UserServiceController> logger)
        {
            _userService = userService;
            _Context = context;
            _logger = logger;
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
                 d.ImgUrl,
                 Clinics = d.Clinics.Select(c => new { c.Examination, c.Location }).ToList()
             })
             .ToListAsync();

                var result = doctors.Select(d => new
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
                    d.ImgUrl,
                    Examination = d.Clinics.FirstOrDefault()?.Examination,
                    Location = d.Clinics.FirstOrDefault()?.Location
                }).ToList();


                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-doctor-by-id")]
        public async Task<IActionResult> GetDoctorByEmailAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Email cannot be empty");

            try
            {
                // Retrieve specific columns from the Doctors table along with related data
                var doctor = await _Context.Doctors
                    .Where(d => d.Id == id)
                    .Select(d => new
                    {
                        d.Email,
                        d.Name,
                        d.Specialization,
                        d.SmallTip,
                        d.AboutDoctor,
                        d.Rate1,
                        d.Rate2,
                        d.Rate3,
                        d.Rate4,
                        d.Rate5,
                        d.PhoneNumber,
                        d.ImgUrl,
                        Clinics = d.Clinics
                            .Where(c => c.DoctorId == d.Id)
                            .Select(c => new
                            {
                                c.Id,
                                c.Location,
                                c.Examination,
                                c.Discription,
                                c.Name,
                                c.Phone,
                                Images = _Context.ClinicImages
                                    .Where(i => i.ClinicId == c.Id)
                                    .Select(i => new { i.ImageUrl })
                                    .ToList(),
                                Assistants = _Context.Assistants
                                    .Where(i => i.ClinicId == c.Id)
                                    .Select(i => new { i.Number, i.Name })
                                    .ToList()
                            })
                            .ToList(),
                        Review = d.Reviews.Where(
                            c => c.DoctorId == d.Id)
                        .Select(c => new
                        {
                            c.date,
                            c.id,
                            c.patient.Name,
                            c.PatientId,
                            c.patient.ImgUrl,
                            c.Likes,
                            c.NumOfStars,
                            c.Dislikes,
                            c.review
                        })
                        .ToList(),
                        Schedule = d.Schedules.Where(s => s.DoctorId == d.Id)
                       .Select(s => new
                       {
                           s.ThisWeek,
                           s.NextWeek,
                           s.NextTwoWeek,
                           s.NextTreeWeek,
                           s.ClinicId,
                           SatShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Sat" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)
                        })
                        .ToList(),
                           SunShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Sun" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

                        })
                        .ToList(),
                           MunShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Mun" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

                        })
                        .ToList(),
                           TueShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Tue" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

                        })
                        .ToList(),
                           WedShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Wed" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

                        })
                        .ToList(),
                           ThuShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Thu" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

                        })
                        .ToList(),
                           FriShifts = s.ScheduleShifts
                          .Where(ss => ss.Shift.Day.ToLower() == "Fri" && ss.ScheduleId == s.Id)
                        .Select(ss => new
                        {
                            ShiftId = ss.Shift.Id,
                            ss.Shift.Day,
                            ss.Shift.StartTime,
                            ss.Shift.EndTime,
                            Available = !_Context.Appointments.Any(a => a.ShiftId == ss.Shift.Id && a.appointmentStatusId == 1)

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

        [HttpPost("add-review")]
        public async Task<IActionResult> AddReviewAsync(ReviewDto review)
        {
            try
            {
                var result = await _userService.AddReviewAsync(review);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a review.");

                // Return a meaningful error response to the client
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }
        [HttpPost("increment-likes")]
        public async Task<IActionResult> IncrementLikesAsync(Increment increment)
        {
            try
            {
                var result = await _Context.Reviews.Where(i => i.id == increment.Id).FirstOrDefaultAsync();
                if (result is not null)
                {

                    result.Likes = increment.newNumber;
                    _Context.Reviews.Update(result);
                    await _Context.SaveChangesAsync();
                    return Ok(true);
                }
                else
                    return Ok(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a review.");

                // Return a meaningful error response to the client
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }
        [HttpPost("increment-dislikes")]
        public async Task<IActionResult> IncrementDislikesAsync(Increment increment)
        {
            try
            {
                var result = await _Context.Reviews.Where(i => i.id == increment.Id).FirstOrDefaultAsync();
                if (result is not null)
                {
                    result.Dislikes = increment.newNumber;
                    _Context.Reviews.Update(result);
                    await _Context.SaveChangesAsync();
                    return Ok(true);
                }
                else
                    return Ok(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a review.");

                // Return a meaningful error response to the client
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }
        [HttpPost("book-appointment")]
        public async Task<IActionResult> AddApointmentAsync(AppointmentDto appointment)
        {
            try
            {
                var patient = await _Context.Patients.Where(p => p.Email == appointment.PatientEmail).FirstOrDefaultAsync();
                var doctor = await _Context.Doctors.Where(p => p.Id == appointment.DoctorId).FirstOrDefaultAsync();
                if (patient != null)
                {
                    //var patientReviewed = await _Context.Appointments.Where(p => p.PatientId == patient.Id).FirstOrDefaultAsync();
                    //if (patientReviewed is null)
                    //{
                    var newAppointment = new Alzheimer.Tables.Appointment();



                    newAppointment.PatientId = patient.Id;
                    newAppointment.DoctorId = appointment.DoctorId;
                    newAppointment.AppDay = appointment.Date;
                    newAppointment.BookingToOther = appointment.BookingToOther;
                    newAppointment.ShiftId = appointment.ShiftId;
                    newAppointment.PatientEmail = appointment.PatientEmail;
                    newAppointment.PatientName = appointment.PatientName;
                    newAppointment.PatientPhone = appointment.PatientPhone;
                    newAppointment.TimeOfAdding = DateTime.Now;
                    newAppointment.appointmentStatusId = 1;
                    var shift = await _Context.Shifts.Where(p => p.Id == appointment.ShiftId).FirstOrDefaultAsync();
                    if (_userService.SendEmail(patient.Name, patient.Email, "Booking Appointment", $"<h1>Hi {patient.Name} </h1><div>You just booked an appointment</div><div>Dr:{patient.Name}</div><a href=''>Time in {appointment.Date}</a><div>for {shift.StartTime} to {shift.EndTime}</div>").IsCompletedSuccessfully)
                        if (_userService.SendEmail(doctor.Name, doctor.Email, "Booking Appointment", $"<h1>Hi Dr:{doctor.Name} </h1><div>It just booked an appointment</div><div>By {doctor.Name}</div><a href=''>Time in {appointment.Date}</a><div>for {shift.StartTime} to {shift.EndTime}</div>").IsCompletedSuccessfully)

                            _Context.Appointments.Add(newAppointment);
                    _Context.SaveChanges();
                    //}
                    return Ok(new stringClass { data = "Your Booking compelelte successfully" });


                }
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a review.");

                // Return a meaningful error response to the client
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }
        [HttpGet("get-appointment-patient")]

        public async Task<IActionResult> GetApointmentAsync(string email)
        {
            try
            {
                var patient = await _Context.Patients.Where(p => p.Email == email).FirstOrDefaultAsync();
                if (patient != null)
                {
                    var appointments = await _Context.Appointments
                        .Where(a => a.PatientId == patient.Id)
                        .Select(a => new
                        {
                            Id = a.Id,
                            AppDay = a.AppDay,
                            BookingToOther = a.BookingToOther,
                            PatientPhone = a.PatientPhone,
                            PatientName = a.PatientName,
                            PatientEmail = a.PatientEmail,
                            PatientId = a.PatientId,
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
                _logger.LogError(ex, "An error occurred while adding a review.");

                // Return a meaningful error response to the client
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("send-message")]

        public async Task<IActionResult> SendMessageFromPatientToDoctor(Message message)
        {
            try
            {
                var patient = _Context.Patients.Where(patient => patient.Email == message.SenderId).FirstOrDefault();
                var chat = new Chat
                {
                    Content = message.Content,
                    Time = DateTime.Now,
                    SenderId = patient.Id,
                    SenderType = Tables.SenderType.Patient,
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
                var patient = _Context.Patients.Where(a => a.Email == email).FirstOrDefault();

                if (patient is not null)
                {
                    var messages = _Context.Chats
                   .Where(m => m.SenderId == patient.Id  || m.ReceiverId == patient.Id ).Select(c => new
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

        [HttpGet("cancel")]
        public async Task<IActionResult> GetChatByEmailAsync(int id)
        {
            try
            {
                var a = _Context.Appointments.Where(a => a.Id == id).FirstOrDefault();

                if (a is not null)
                {
                    a.appointmentStatusId = 2;
                    _Context.Update(a);
                    _Context.SaveChanges();
                    var doctor = _Context.Doctors.Where(d => d.Id == a.DoctorId).FirstOrDefault();
                    var shift = _Context.Shifts.Where(s => s.Id == a.ShiftId).FirstOrDefault();
                    if (_userService.SendEmail(doctor.Name, doctor.Email, "Cancel Appointment", $"<h1>Hi Dr: {doctor.Name} </h1><div>{a.PatientName} just Cancel an appointment</div><div> Day : {a.AppDay} At Shift From {shift.StartTime} to {shift.EndTime}</div>").IsCompletedSuccessfully) ;

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
