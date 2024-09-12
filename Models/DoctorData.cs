namespace Alzaheimer.Models
{
    public class DoctorData
    {
        public string email { get; set; }
        public string name { get; set; }
        public string imgUrl { get; set; }
        public List<Clinic> clinics { get; set; }
        public List<Schedule> schedule { get; set; }
    }

    public class Clinic
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Schedule
    {
        public int clinicId { get; set; }
        public List<DayShift> satShifts { get; set; }
        public List<DayShift> sunShifts { get; set; }
        public List<DayShift> monShifts { get; set; }
        public List<DayShift> tueShifts { get; set; }
        public List<DayShift> wedShifts { get; set; }
        public List<DayShift> thuShifts { get; set; }
        public List<DayShift> friShifts { get; set; }
    }

    public class DayShift
    {
        public int id { get; set; }
        public int shiftId { get; set; }
        public string day { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<Appointment> appointments { get; set; }
    }

    public class Appointment
    {
        public string patientId { get; set; }
        public int appointmentStatusId { get; set; }
        public DateTime appDay { get; set; }
    }
}
