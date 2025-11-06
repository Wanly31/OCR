namespace OCR.Models.Domain
{
    public class RecognizedText
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public Guid DocumentId { get; set; }
        public Document Document { get; set; }

        public RecognizedDocument? RecognizedDocument { get; set; }
    }
}
