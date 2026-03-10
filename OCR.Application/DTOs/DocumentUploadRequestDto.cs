using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OCR.Application.DTOs
{
    public class DocumentUploadRequestDto
    {
        [Required]
        public IFormFile File { get; set; }
        [Required]
        public string FileName { get; set; }
        public string? FileDescription { get; set; }
    }
}
