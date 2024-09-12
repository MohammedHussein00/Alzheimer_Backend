namespace Alzaheimer.Models
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string ImgUrl { get; set; }


        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
        public bool EmailConfirmed { get; set; }

    }
}
