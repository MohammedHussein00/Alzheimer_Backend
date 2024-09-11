using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Discription { get; set; }
        public int Examination { get; set; }
        public ICollection<ClinicImage> ClinicImages { get; set; }
        public ICollection<Assistant> Assistants { get; set; }
        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
    }
}
