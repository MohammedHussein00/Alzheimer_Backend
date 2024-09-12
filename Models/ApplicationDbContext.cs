using Alzaheimer.Tables;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Alzheimer.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScheduleShift>().HasKey(t => new { t.ScheduleId, t.ShiftId });
            modelBuilder.Entity<ScheduleShift>().HasOne(p => p.Schedule).WithMany(p => p.ScheduleShifts).HasForeignKey(p => p.ScheduleId);
            modelBuilder.Entity<ScheduleShift>().HasOne(p => p.Shift).WithMany(p => p.ScheduleShifts).HasForeignKey(p => p.ShiftId);



        }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<ClinicImage> ClinicImages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<ScheduleShift> scheduleShifts { get; set; }
        public DbSet<Detection> Detections { get; set; }
        public DbSet<DetectionMRI> DetectionMRIs { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }

    }
}
