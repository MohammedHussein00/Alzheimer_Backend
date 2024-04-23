namespace Alzheimer.Tables
{
    public class Appointment
    {
        public int Id { get; set; }
        public string AppDay { get; set; }
        public string AppTime { get; set; }
        public string PatientId { get; set; }

        public Patient? patient { get; set; }
        public int DoctorId { get; set; }
        public Doctor? doctor { get; set; }
    }
}
