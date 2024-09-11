namespace Alzaheimer.Tables
{
    public class ScheduleShift
    {
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public int ShiftId { get; set; }
        public Shift Shift { get; set; }
    }
}
