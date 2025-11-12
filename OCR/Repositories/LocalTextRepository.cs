using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;

namespace OCR.Repositories
{
    public class LocalTextRepository : ITextRepository
    {

        private readonly OCRDbContext dbContext;

        public LocalTextRepository(OCRDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<Document?> GetByIdAsync(Guid id)
        {
            return await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        }

        // Зберегти розпізнаний текст
        public async Task SaveRecognizedTextAsync(RecognizedText text)
        {
            await dbContext.RecognizedTexts.AddAsync(text);
            await dbContext.SaveChangesAsync();
        }
    }
}
