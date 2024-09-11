using Alzaheimer.Tables;

namespace Alzheimer.Tables
{
    public class Doctor:User
    {
        public string Specialization { get; set; }
        public string SmallTip { get; set; }
        public string AboutDoctor { get; set; }
        public int Rate1 { get; set; }
        public int Rate2 { get; set; }
        public int Rate3 { get; set; }
        public int Rate4 { get; set; }
        public int Rate5 { get; set; }
        public ICollection<Clinic> Clinics { get; set; }
        public ICollection<Education> Educations { get; set; }
        public ICollection<Detection> Detections { get; set; }
        public ICollection<Experience> Experiences { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Chat> Chats { get; set; }



    }
}
