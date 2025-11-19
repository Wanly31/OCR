using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;
using OCR.Models.DTO;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Repositories
{
    public class LocalRecognizeTextRepository : IRecognizeTextRepository
    {
        private readonly OCRDbContext dbContext;

        public LocalRecognizeTextRepository(OCRDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<RecognizeText> DeleteAsync(Guid id)
        {
            var existingText = await dbContext.RecognizedDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (existingText == null)
            {
                return null;
            }

            dbContext.RecognizedDocuments.Remove(existingText);
            await dbContext.SaveChangesAsync();
            return existingText;
        }

        public async Task<List<RecognizeText>> GetAllAsync()
        {
            var text = dbContext.RecognizedDocuments.AsQueryable();
            return await text.ToListAsync();
        }

        public async Task<Recognize> GetByIdAsync (Guid id){
            return await dbContext.RecognizedTexts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RecognizeText> GetByIdTextAsync(Guid id)
        {
            return await dbContext.RecognizedDocuments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task SaveRecognizedTextAsync(RecognizeText text)
        {
            await dbContext.RecognizedDocuments.AddAsync(text);
            await dbContext.SaveChangesAsync();
        }

        public async Task<RecognizeText> UpdateAsync(Guid id, RecognizeText textDomainModel)
        {
            var existingText = await dbContext.RecognizedDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (existingText == null)
            {
                return null;
            }
            
            existingText.FirstName = textDomainModel.FirstName;
            existingText.LastName = textDomainModel.LastName;
            existingText.Medicine = textDomainModel.Medicine;
            existingText.Treatment = textDomainModel.Treatment;
            existingText.DateDocument = textDomainModel.DateDocument;
            
            await dbContext.SaveChangesAsync();
            return existingText;
        }
    }
}
