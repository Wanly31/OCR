using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Application.DTOs
{
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