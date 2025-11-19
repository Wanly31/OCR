using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;

namespace OCR.Repositories
{
    public class LocalRecognizeRepository : IRecognizeRepository
    {

        private readonly OCRDbContext dbContext;

        public LocalRecognizeRepository(OCRDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Recognize> DeleteAsync(Guid id)
        {
            var existingRecognize = await dbContext.RecognizedTexts.FirstOrDefaultAsync(x => x.Id == id);
            if(existingRecognize == null)
            {
                return null;
            }

            dbContext.RecognizedTexts.Remove(existingRecognize);
            await dbContext.SaveChangesAsync();
            return existingRecognize;

        }

        public async Task<List<Recognize>> GetAllAsync()
        {
            var text = dbContext.RecognizedTexts.AsQueryable();
            return await text.ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            return await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recognize> GetByIdTextAsync(Guid id)
        {
            return await dbContext.RecognizedTexts.FirstOrDefaultAsync(x => x.Id == id);
        }

        // Зберегти розпізнаний текст
        public async Task SaveRecognizedTextAsync(Recognize text)
        {
            await dbContext.RecognizedTexts.AddAsync(text);
            await dbContext.SaveChangesAsync();
        }
    }
}
