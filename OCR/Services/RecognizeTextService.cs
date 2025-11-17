using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Services
{
    public class RecognizeTextService
    {
        public Task<String> RecognizeText(string text)
        {
            Regex data = new Regex("\\b\\d{2}.\\d{2}.\\d{4}\\b"); // пошук дати XX-XX-XXXX
            var Data = data.Matches(text).FirstOrDefault();

            Regex name = new Regex(@"\b([A-Z][a-z]+)\s(?:([A-Z]\.?\s)?([A-Z][a-z]+(?:-[A-Z][a-z]+)?))\b"); // пошук імені 
            var Name = name.Matches(text).FirstOrDefault();

            Regex medicine = new Regex(""); // пошук ліків
            var Medicine = medicine.Matches(text).FirstOrDefault();

            Regex treatment = new Regex(""); // пошук заборонених ліків
            var Treatment = treatment.Matches(text).FirstOrDefault();

            return Task.FromResult($"{Data?.Value} {Name?.Value}");

        }

    }
}
