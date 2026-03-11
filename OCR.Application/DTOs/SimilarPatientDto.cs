namespace OCR.Application.DTOs
{
    public class SimilarPatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int RecordCount { get; set; }
    }
}
