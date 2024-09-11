using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class AppointmentStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public ICollection<Appointment> Appointments { get; set; }

    }
}
