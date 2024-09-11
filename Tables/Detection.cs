using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Detection
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? DoctorId { get; set; }
        public Doctor? doctor { get; set; }
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public ICollection<DetectionMRI> DetectionMRIs { get; set; }

    }
}
