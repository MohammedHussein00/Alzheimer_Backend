namespace Alzaheimer.Models
{
    public class UpdatePateint
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Phone { get; set; }
        public IFormFile? Photo { get; set; }
        public string CurrentEmail { get; set; }
    }
}
