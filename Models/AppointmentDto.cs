namespace Alzaheimer.Models
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public bool BookingToOther { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string PatientEmail { get; set; }
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }

        public string DoctorId { get; set; }
    }
}
