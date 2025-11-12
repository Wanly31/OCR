using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;

namespace OCR.Repositories
{
    public class LocalRecognizeTextRepository : IRecognizeTextRepository
    {
        private readonly OCRDbContext dbContext;

        public LocalRecognizeTextRepository(OCRDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<RecognizedText> GetByIdAsync (Guid id){
            return await dbContext.RecognizedTexts.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
