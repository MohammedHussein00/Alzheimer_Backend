using Alzheimer.Tables;

namespace Alzaheimer.Tables
{
    public class Chat
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
        public string SenderId { get; set; }  // Can be PatientId, DoctorId, or AdminId
        public SenderType SenderType { get; set; }  // Indicates whether sender is Patient, Doctor, or Admin
        public string ReceiverId { get; set; }  // DoctorId or AdminId, depending on sender type
        public bool Status { get; set; }  

        // Navigation properties
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
    public enum SenderType
    {
        Patient,
        Doctor,
        Admin  // If admin can also send messages
    }

}
