namespace OCR.Models.DTO
{
    public class RecognizeTextDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Medicine { get; set; }
        public string? Treatment { get; set; }
        public DateOnly? DateDocument { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
