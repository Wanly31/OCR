namespace OCR.Application.DTOs
{
    public class RecognizedDataDto
    {
        public string? Examination { get; set; }
        public string? Medicine { get; set; }
        public string? Treatment { get; set; }
        public string? ContraindicatedMedicine { get; set; }
        public string? ContraindicatedReason { get; set; }
        public DateOnly? DateDocument { get; set; }
    }
}
