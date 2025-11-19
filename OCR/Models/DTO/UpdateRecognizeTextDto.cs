namespace OCR.Models.DTO
{
    public class UpdateRecognizeTextDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Medicine { get; set; }
        public string? Treatment { get; set; }
        public DateOnly? DateDocument { get; set; }
    }
}
