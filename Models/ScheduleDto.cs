namespace Alzaheimer.Models
{
    public class ScheduleDto
    {
        public string Email { get; set; }
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public bool ThisWeek { get; set; }
        public bool NextWeek { get; set; }
        public bool NextTwoWeek { get; set; }
        public bool NextTreeWeek { get; set; }
        public List<ShiftDto> Shifts { get; set; }

    }
}
