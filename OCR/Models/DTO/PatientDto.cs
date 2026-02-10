namespace OCR.Models.DTO
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int TotalRecords { get; set; }  // Кількість медичних записів
    }

    public class PatientSearchRequestDto
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

    public class PatientConfirmationDto
    {
        public Guid? ExistingPatientId { get; set; }  // null = створити нового пацієнта
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Guid RecognizedId { get; set; }  // ID розпізнаного тексту
        public RecognizedDataDto RecognizedData { get; set; }  // Медичні дані
    }

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
