namespace Alzaheimer.Models
{
    public class UpdateDoctor
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Phone { get; set; }
        public string SmallTip { get; set; }
        public string Specialization { get; set; }
        public string AboutDoctor { get; set; }
        public IFormFile? Photo { get; set; }
        public string CurrentEmail { get; set; }
    }
}
