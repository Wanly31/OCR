using System.ComponentModel.DataAnnotations;

namespace OCR.Models.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }
}
