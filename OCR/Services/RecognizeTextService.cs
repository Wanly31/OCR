using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Services
{
    public class RecognizeTextService
    {
        public Task<String> RecognizeText(string text)
        {
            Regex regex = new Regex("\\b\\d{2}.\\d{2}.\\d{4}\\b"); // Приклад: пошук шаблону у форматі XX-XX-XXXX
            var match = regex.Matches(text).FirstOrDefault();
            return Task.FromResult(match?.Value);
        }

    }
}
