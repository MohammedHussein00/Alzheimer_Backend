namespace Alzaheimer.Tables
{
    public class DetectionMRI
    {
        public int Id { get; set; }
        public string Result { get; set; }

        public int DetectionId { get; set; } // Foreign key to Detection
        public Detection? Detection { get; set; } // Navigation property
        public string MRIUrl { get; set; } // Composed attribute representing a result
    }
}
