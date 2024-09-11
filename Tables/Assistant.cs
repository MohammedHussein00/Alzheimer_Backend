namespace Alzaheimer.Tables
{
    public class Assistant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public int ClinicId { get; set; }
        public Clinic? clinic { get; set; }
    }
}
