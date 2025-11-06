namespace OCR.Models.Domain
{
    public class RecognizedDocument
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Medicine { get; set; }
        public string Treatment { get; set; }
        public DateOnly DateDocument { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Foreign key   
        public Guid RecognizedTextId { get; set; }
        public RecognizedText RecognizedText { get; set; }

    }
}
