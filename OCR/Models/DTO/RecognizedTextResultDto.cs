namespace OCR.Models.DTO
{
    /// <summary>
    /// DTO для результатів розпізнавання тексту з OCR
    /// </summary>
    public class RecognizedTextResultDto
    {
        // Дані пацієнта
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }

        // Медичні дані
        public string? Examination { get; set; }
        public string? Medicine { get; set; }
        public string? Treatment { get; set; }
        public string? ContraindicatedMedicine { get; set; }
        public string? ContraindicatedReason { get; set; }
        public DateOnly? DateDocument { get; set; }
    }
}
