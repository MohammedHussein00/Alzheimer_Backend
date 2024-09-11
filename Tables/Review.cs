namespace Alzheimer.Tables
{
    public class Review
    {
        public int id { get; set; }
        public string review { get; set; }
        public string date { get; set; }
        public int NumOfStars { get; set; }

        public string PatientId { get; set; }

        public Patient? patient { get; set; }
        public string DoctorId { get; set; }
        public Doctor? doctor { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }

    }
}
