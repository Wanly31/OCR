using OCR.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Application.Interfaces
{
    public interface IRecognizeTextService
    {
        Task<RecognizedTextResultDto> RecognizeTextAsync(string filePath);
    }
}
