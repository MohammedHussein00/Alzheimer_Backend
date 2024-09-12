using Alzaheimer.Tables;

namespace Alzaheimer.Models
{
    public class Message
    {

        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
        public string SenderId { get; set; }  // Can be PatientId, DoctorId, or AdminId
        public string ReceiverId { get; set; }  // DoctorId or AdminId, depending on sender type
    }
 
}
