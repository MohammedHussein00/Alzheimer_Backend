using Alzaheimer.Tables;

namespace Alzaheimer.Models
{
    public class DetectionDto
    {
        public string? DoctorEmail { get; set; }
        public string? PatientEmail { get; set; }
        public List<DetectionResultDto> DetectionResult { get; set; }
    }
}
