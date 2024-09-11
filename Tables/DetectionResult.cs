namespace Alzaheimer.Tables
{
    public class DetectionResult
    {
        public int Id { get; set; }
        public int DetectionId { get; set; } // Foreign key to Detection
        public Detection? Detection { get; set; } // Navigation property
        public int Result { get; set; } // Composed attribute representing a result
    }
}
