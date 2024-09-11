using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Experience
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PlaceWorkName { get; set; }
        public string JobDescription { get; set; }
        public string PlaceWorkLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
    }
}
