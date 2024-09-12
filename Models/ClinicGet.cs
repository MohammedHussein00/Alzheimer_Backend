using Alzaheimer.Tables;

namespace Alzaheimer.Models
{
    public class ClinicGet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public int Examination { get; set; }
        public List<Assist> Assist { get; set; }
        public List<ImageDto> Images { get; set; }
    }
}
