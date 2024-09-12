namespace Alzaheimer.Models
{
    public class RegisterDoctor
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string DOB { get; set; }
        public string Phone { get; set; }
        public string Specialization { get; set; }
        public IFormFile Certification { get; set; }
    }
}
