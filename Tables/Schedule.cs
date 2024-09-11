using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Schedule
    {
        public int Id { get; set; }
        // Navigation property

        public bool ThisWeek { get; set; }
        public bool NextWeek { get; set; }
        public bool NextTwoWeek { get; set; }
        public bool NextTreeWeek { get; set; }
        public List<ScheduleShift> ScheduleShifts { get; set; }
        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
        public int  ClinicId { get; set; }
        public Clinic? clinic { get; set; }
    }
}
