namespace OCR.Models.Domain
{
    public class RecognizeText
    {
        public Guid Id { get; set; }

        // Foreign key до Patient
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } 

        // Медичні дані
        public string? Examination { get; set; }
        public string? Medicine { get; set; }
        public string? Treatment { get; set; }
        public string? ContraindicatedMedicine { get; set; }
        public string? ContraindicatedReason { get; set; }
        public DateOnly? DateDocument { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        //Foreign key до Recognize  
        public Guid RecognizedTextId { get; set; }
        public Recognize RecognizedText { get; set; }

    }
}
