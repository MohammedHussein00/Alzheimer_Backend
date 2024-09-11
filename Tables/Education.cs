using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Education
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string University { get; set; }
        public int Year { get; set; }
        public string certification { get; set; }
        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
    }
}
