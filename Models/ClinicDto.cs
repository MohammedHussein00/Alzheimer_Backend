using Alzaheimer.Tables;

namespace Alzaheimer.Models
{
    public class ClinicDto
    {
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public int Examination { get; set; }
        public List<Assist> Assist { get; set; }
        public List<ClinicImageUpdate> Images { get; set; }
        public List<string>? ImagesURL { get; set; }
    }
}
