using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Shift
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        // Navigation property
        public ICollection<Appointment> Appointments { get; set; }

        public List<ScheduleShift> ScheduleShifts { get; set; }
    }
}
