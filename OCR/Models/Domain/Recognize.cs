namespace OCR.Models.Domain
{
    public class Recognize
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public Guid DocumentId { get; set; }
        public Document Document { get; set; }

        public RecognizeText? RecognizedDocument { get; set; }
    }
}
