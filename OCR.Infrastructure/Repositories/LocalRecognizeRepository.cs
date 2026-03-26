using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR.Domain.Entities;
using OCR.Application.Abstractions;
using OCR.Infrastructure.Data;

namespace OCR.Infrastructure.Repositories
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

        public async Task<Recognize?> GetByIdAsync(Guid id)
        {
            return await dbContext.RecognizedTexts
                .Include(r => r.RecognizedDocument)
                .FirstOrDefaultAsync(x => x.DocumentId == id);
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

        public async Task<Recognize> UpdateAsync(Guid id, Recognize textDomainModel)
        {
            var existingText = await dbContext.RecognizedTexts.FirstOrDefaultAsync(x => x.Id == id);

            if(existingText == null)
            {
                return null;
            }

            existingText.Text = textDomainModel.Text;

            await dbContext.SaveChangesAsync();
            return existingText;
        }
    }
}
