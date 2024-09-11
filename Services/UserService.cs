using Alzaheimer.Models;
using Alzheimer.Helpers;
using Alzheimer.Models;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using Tensorflow.Eager;

namespace Alzaheimer.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly JWT _Jwt;


        public UserService(UserManager<IdentityUser> userManager
            , IOptions<JWT> jwt
            , RoleManager<IdentityRole> roleManager
            , ApplicationDbContext Context)
        {
            _Context = Context;
            _userManager = userManager;
            _Jwt = jwt.Value;
            _roleManager = roleManager;
        }
        public async Task<string> AddAppointmentAsync(AppointmentDto model)
        {
            var patient = await _Context.Patients.Where(p => p.Email == model.PatientEmail).FirstOrDefaultAsync();
            //var doctor = await _Context.Doctors.Where(p => p.Email == model.DoctorId).FirstOrDefaultAsync();
            if (patient != null)
            {
                //var patientReviewed = await _Context.Appointments.Where(p => p.PatientId == patient.Id).FirstOrDefaultAsync();
                //if (patientReviewed is null)
                //{
                var newAppointment = new Alzheimer.Tables.Appointment();



                newAppointment.PatientId = patient.Id;
                newAppointment.DoctorId = model.DoctorId;
                newAppointment.AppDay = model.Date;
                newAppointment.BookingToOther = model.BookingToOther;
                newAppointment.ShiftId = model.ShiftId;
                newAppointment.PatientEmail = model.PatientEmail;
                newAppointment.PatientName = model.PatientName;
                newAppointment.PatientPhone = model.PatientPhone;


                _Context.Appointments.Add(newAppointment);
                _Context.SaveChanges();
                //}
                return "Your Booking compelelte successfully";


            }
            else
                return "";

        }

        public async Task<string> AddReviewAsync(ReviewDto model)
        {
            var patient = await _Context.Patients.Where(p => p.Email == model.PatientEmail).FirstOrDefaultAsync();
            var doctor = await _Context.Doctors.Where(p => p.Email == model.DoctorId).FirstOrDefaultAsync();
            if (patient != null && doctor != null)
            {
                var patientReviewed = await _Context.Reviews.Where(p => p.PatientId == patient.Id).FirstOrDefaultAsync();
                if (patientReviewed is not null)
                    return "You reviewed this Doctor before";

                var newReview = new Review
                {


                    PatientId = patient.Id,
                    review = model.review,
                    DoctorId = doctor.Id,
                    NumOfStars = model.NumOfStars,
                    date = (DateTime.Now).ToString("yyyy-MM-dd hh:mm tt"),
                    Likes = 0,
                    Dislikes = 0
                };
                if (model.NumOfStars == 1)
                    doctor.Rate1++;
                else if (model.NumOfStars == 2)
                    doctor.Rate2++;
                else if (model.NumOfStars == 3)
                    doctor.Rate3++;
                else if (model.NumOfStars == 4)
                    doctor.Rate4++;
                else if (model.NumOfStars == 5)
                    doctor.Rate5++;
                _Context.Doctors.Update(doctor);
                _Context.Add(newReview);
                _Context.SaveChanges();
                return "";

            }
            else
                return "";

        }
        public async Task<bool> SendEmail(string userName, string userEmail, string subject, string textContent)
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
                var sendSmtpEmail = new SendSmtpEmail(sender, recipients, null, null, textContent, "xdfxg", subject); // Pass an empty list for Bcc

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


