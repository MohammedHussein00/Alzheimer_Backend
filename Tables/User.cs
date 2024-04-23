using Microsoft.AspNetCore.Identity;

namespace Alzheimer.Tables
{
    public class User:IdentityUser
    {
        public string? Name { get; set; }
        public string DOB { get; set; }

        public string? ImgUrl { get; set; }

    }
}
