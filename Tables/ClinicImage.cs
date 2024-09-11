namespace Alzaheimer.Tables
{
    public class ClinicImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int ClinicId { get; set; }
        public Clinic? clinic { get; set; }
    }
}
