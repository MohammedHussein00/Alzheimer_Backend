using Alzaheimer.Tables;

namespace Alzheimer.Tables
{
    public class Patient:User
    {
        //public ICollection<Review> Reviews { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Chat> Chats { get; set; }

    }
}
