using Alzaheimer.Models;
using Alzaheimer.Tables;
using Alzheimer.Helpers;
using Alzheimer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Alzaheimer.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly JWT _Jwt;
        private readonly IWebHostEnvironment _hostingEnvironment;



        public DoctorService(IWebHostEnvironment hostingEnvironment, UserManager<IdentityUser> userManager
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
        public async Task<ClinicGet> UpdateClinicAsync(ClinicDto model)
        {
            int id = 0;
            int count = 0;
            var clinics = new ClinicGet();

            var user = await _userManager.FindByEmailAsync(model.Email);
            //clinics[count].Email = user.Email;

            var clinic = await _Context.Clinics.Where(m => m.Id == model.Id).FirstOrDefaultAsync();
            if (clinic == null)
            {
                // Create a new clinic if it doesn't exist
                clinic = new Tables.Clinic();
                clinic.Name = model.Name;
                clinic.Location = model.Location;
                clinic.Examination = model.Examination;
                clinic.Discription = model.Description;
                clinic.Phone = model.Phone;
                clinic.DoctorId = user.Id;
                _Context.Clinics.Add(clinic);
                await _Context.SaveChangesAsync();
                var temp = await _Context.Clinics
                    .Where(c => c.Name == model.Name && c.Location == model.Location && c.Phone == model.Phone)
                    .FirstOrDefaultAsync();
                var schedule = new Tables.Schedule();
                schedule.ThisWeek = false;
                schedule.NextWeek = false;
                schedule.NextTwoWeek = false;
                schedule.NextTreeWeek = false;
                schedule.ClinicId = temp.Id;
                schedule.DoctorId = user.Id;
                _Context.Schedules.Add(schedule);
                await _Context.SaveChangesAsync();
                var imageUrls = new List<string>();
                foreach (var imageFile in model.Images)
                {
                    // Process each IFormFile object and extract relevant information
                    // For example, if you want to get the file name:
                    var fileName = imageFile.Image.FileName;
                    imageUrls.Add(fileName);
                }


                int count2 = 0;
                if (temp is not null)
                {
                    var cID = temp.Id;
                    id = temp.Id;
                    // Add assistants to the clinic
                    foreach (var assist in model.Assist)
                    {
                        var assistant = new Assistant();

                        assistant.Name = assist.Name;
                        assistant.Number = assist.Phone;
                        assistant.ClinicId = cID;


                        _Context.Assistants.Add(assistant);
                        await _Context.SaveChangesAsync();


                    }
                    count2 = 0;
                    foreach (var img in model.Images)
                    {

                        var image = new ClinicImage();
                        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DoctorData");

                        image.ImageUrl = $"{folderPath}\\{model.Email}\\{img.Image.FileName}{Path.GetExtension(img.Image.FileName)}";
                        image.ClinicId = cID;
                        _Context.ClinicImages.Add(image);
                        await _Context.SaveChangesAsync();

                        SaveFile(image.ImageUrl, img.Image);

                        model.Id = cID;
                        var lastAssist = await _Context.ClinicImages
                        .OrderByDescending(c => c.Id) // Assuming 'Id' is an auto-increment primary key
                        .FirstOrDefaultAsync();
                        for (int j = 0; j < model.Assist.Count; j++)
                        {
                            if (model.Assist[j].Id == 0 && lastAssist != null)
                            {
                                model.Images[j].Id = lastAssist.Id;
                                break;
                            }
                        }

                        count++;


                    }

                    // Add the new clinic to the context
                }
            }
            else
            {

                clinic.Name = model.Name;
                clinic.Location = model.Location;
                clinic.Examination = model.Examination;
                clinic.Discription = model.Description;
                clinic.Phone = model.Phone;
                clinic.DoctorId = user.Id;
                _Context.Clinics.Update(clinic);
                await _Context.SaveChangesAsync();
                var cID = model.Id;


                int count2 = 0;
                foreach (var assist in model.Assist)
                {
                    var a = await _Context.Assistants.Where(a => a.Id == assist.Id).FirstOrDefaultAsync();

                    if (a is null)
                    {
                        var assistant = new Assistant();

                        assistant.Name = assist.Name;
                        assistant.Number = assist.Phone;
                        assistant.ClinicId = cID;
                        _Context.Assistants.Add(assistant);
                        var lastAssist = await _Context.Assistants
                                           .Where(c => c.ClinicId == cID)
                                           .OrderByDescending(c => c.Id)
                                           .FirstOrDefaultAsync();
                        for (int i = 0; i < model.Assist.Count; i++)
                        {
                            if (model.Assist[i].Id == 0 && lastAssist != null)
                            {
                                model.Assist[i].Id = lastAssist.Id;
                                break;
                            }
                        }

                    }
                    else
                    {
                        a.Name = assist.Name;
                        a.Number = assist.Phone;
                        a.ClinicId = cID;

                        _Context.Assistants.Update(a);

                    }

                    await _Context.SaveChangesAsync();



                }
                count2 = 0;
                foreach (var img in model.Images)
                {
                    var i = await _Context.ClinicImages.Where(a => a.Id == img.Id).FirstOrDefaultAsync();
                    if (i is null)
                    {
                        var image = new ClinicImage();
                        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DoctorData");

                        image.ImageUrl = $"{folderPath}\\{model.Email}\\{img.Image.FileName}{Path.GetExtension(img.Image.FileName)}";
                        image.ClinicId = cID;
                        _Context.ClinicImages.Add(image);
                        if (!File.Exists(image.ImageUrl))
                        {
                            // Open a stream to write the file content
                            using (var stream = new FileStream(image.ImageUrl, FileMode.Create))
                            {
                                // Copy the file content to the stream asynchronously
                                await img.Image.CopyToAsync(stream);
                            }
                        }
                        var lastAssist = await _Context.ClinicImages
                         .OrderByDescending(c => c.Id) // Assuming 'Id' is an auto-increment primary key
                         .FirstOrDefaultAsync();
                        for (int j = 0; j < model.Images.Count; j++)
                        {
                            if (model.Images[j].Id == 0 && lastAssist != null)
                            {
                                model.Images[j].Id = lastAssist.Id;
                                break;
                            }
                        }



                    }

                }



            }

            var Assists = await _Context.Assistants.Where(a => a.ClinicId == model.Id).ToListAsync();

            // Find the Assistants entries that are not in model.Assists
            var AssistsToDelete = Assists.Where(a => !model.Assist.Any(assist => assist.Id == a.Id)).ToList();

            // Delete the identified Assistants entries
            foreach (var assist in AssistsToDelete)
            {
                _Context.Assistants.Remove(assist);
            }

            var Images = await _Context.ClinicImages.Where(a => a.ClinicId == model.Id).ToListAsync();

            // Find the Assistants entries that are not in model.Assists
            var ImagesToDelete = Images.Where(a => !model.Images.Any(assist => assist.Id == a.Id) && !model.ImagesURL.Any(assist => assist == a.ImageUrl)).ToList();

            // Delete the identified Assistants entries
            foreach (var image in ImagesToDelete)
            {
                _Context.ClinicImages.Remove(image);
                if (File.Exists(image.ImageUrl))
                {
                    // Delete the file
                    File.Delete(image.ImageUrl);
                }
            }



            await _Context.SaveChangesAsync();




            return clinics;

        }






        public async Task<ScheduleDto> UpdateSchedulAsync(ScheduleDto model)
        {
            int id = 0;
            int count = 0;

            var user = await _userManager.FindByEmailAsync(model.Email);
            var clinic = await _Context.Clinics.Where(m => m.Id == model.ClinicId).FirstOrDefaultAsync();
            if (user is null && clinic is null)
                return model;
            var sched = await _Context.Schedules.Where(m => m.Id == model.Id).FirstOrDefaultAsync();

            if (sched is not null)
            {
                sched.ThisWeek = model.ThisWeek;
                sched.NextWeek = model.NextWeek;
                sched.NextTwoWeek = model.NextTwoWeek;
                sched.NextTreeWeek = model.NextTreeWeek;
                sched.ClinicId = model.ClinicId;
                sched.DoctorId = user.Id;
                _Context.Schedules.Update(sched);
                await _Context.SaveChangesAsync();

                var cID = model.Id;


                int count2 = 0;
                foreach (var shift in model.Shifts)
                {
                    var shiftadd = await _Context.Shifts.Where(a => a.Id==shift.Id).FirstOrDefaultAsync();

                    if (shiftadd is null)
                    {
                        var newShift = new Shift();
                        newShift.EndTime = shift.EndTime;
                        newShift.StartTime = shift.StartTime;
                        newShift.Day = shift.Day;

                        _Context.Shifts.Add(newShift);
                        await _Context.SaveChangesAsync();

                        var lastShift = await _Context.Shifts
                        .OrderByDescending(c => c.Id) // Assuming 'Id' is an auto-increment primary key
                        .FirstOrDefaultAsync();
                        var shecduleShift = new ScheduleShift();
                        shecduleShift.ScheduleId = sched.Id;
                        shecduleShift.ShiftId = lastShift.Id;
                        _Context.scheduleShifts.Add(shecduleShift);
                         _Context.SaveChanges();
                        for (int i = 0; i < model.Shifts.Count; i++)
                        {
                            if (model.Shifts[i].Id == 0 && lastShift != null)
                            {
                                model.Shifts[i].Id = lastShift.Id;
                                break;
                            }
                        }


                    }
                    else
                    {
                        var scheduleShift = await _Context.scheduleShifts.Where(a => a.ScheduleId == model.Id && a.ShiftId == shift.Id).FirstOrDefaultAsync();
                        if (scheduleShift is null)
                        {
                            _Context.Add(new ScheduleShift { ShiftId = shift.Id, ScheduleId = model.Id });
                            await _Context.SaveChangesAsync();

                        }

                    }





                }
            }

            var scheduleShiftDelete = await _Context.scheduleShifts.Where(a => a.ScheduleId == model.Id).ToListAsync();

            // Find the scheduleShift entries that are not in model.Shifts
            var scheduleShiftsToDelete = scheduleShiftDelete.Where(scheduleShift => !model.Shifts.Any(shift => shift.Id == scheduleShift.ShiftId)).ToList();

            // Delete the identified scheduleShift entries
            foreach (var scheduleShift in scheduleShiftsToDelete)
            {
                _Context.scheduleShifts.Remove(scheduleShift);
            }





            await _Context.SaveChangesAsync();




            return model;

        }
        public async Task<DetectionDto> DetectionAsync(DetectionDto model)
        {
            var patient = await _Context.Patients.Where(p => p.Email == model.PatientEmail).FirstOrDefaultAsync();

            var doctor = await _Context.Doctors.Where(d => d.Email == model.DoctorEmail).FirstOrDefaultAsync();
            if (doctor is not null && patient is not null)
            {
                var detection = new Detection();
                if (doctor is not null)
                    detection.DoctorId = doctor.Id;
                if (patient is not null)
                    detection.PatientId = patient.Id;
                detection.Date = DateTime.Now;
                _Context.Add(detection);
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
                        _Context.DetectionMRIs.Add(detectionMRI);
                        if (!File.Exists(detectionMRI.MRIUrl))
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

            return model;
        }

        public static async Task SaveFile(string path, IFormFile file)
        {
            // Check if the file and path are valid
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid file or path.");
            }

            // Create directory if it doesn't exist
            if (!File.Exists(path))
            {
                // Open a stream to write the file content
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    // Copy the file content to the stream asynchronously
                    await file.CopyToAsync(stream);
                }
            }
        }



    }
}
