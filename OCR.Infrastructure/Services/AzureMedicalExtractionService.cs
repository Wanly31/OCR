using OCR.Application.Abstractions;
using OCR.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Infrastructure.Services
{
    public class AzureMedicalTextExtractionService : IMedicalExtractionService
    {

        Task<RecognizedTextResultDto> IMedicalExtractionService.ExtractMedicalDataAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
}
