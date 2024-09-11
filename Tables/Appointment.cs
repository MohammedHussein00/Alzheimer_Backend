using Alzaheimer.Tables;
using System.Text.Json.Serialization;

namespace Alzheimer.Tables
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppDay { get; set; }
        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public string PatientName { get; set; }
        public string PatientEmail { get; set; }
        public string PatientPhone { get; set; }
        public bool BookingToOther { get; set; }
        public DateTime TimeOfAdding { get; set; }
        public string PatientId { get; set; }

        [JsonIgnore] // Ignore serialization of the patient property
        public Patient? patient { get; set; }

        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
        public int appointmentStatusId { get; set; }
        public AppointmentStatus? appointmentStatus { get; set; }
    }
}
