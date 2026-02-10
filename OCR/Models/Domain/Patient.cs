namespace OCR.Models.Domain
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - один пацієнт може мати багато медичних записів
        public ICollection<RecognizeText> MedicalRecords { get; set; } = new List<RecognizeText>();
    }
}
