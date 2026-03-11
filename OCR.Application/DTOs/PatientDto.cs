namespace OCR.Application.DTOs
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


}
