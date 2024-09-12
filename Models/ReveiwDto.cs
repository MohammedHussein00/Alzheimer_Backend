using Alzheimer.Tables;

namespace Alzaheimer.Models
{
    public class ReviewDto
    {
        public string review { get; set; }
        public int NumOfStars { get; set; }
        public string PatientEmail { get; set; }

        public string DoctorId { get; set; }
    }
}
