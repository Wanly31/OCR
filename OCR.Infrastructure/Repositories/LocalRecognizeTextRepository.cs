using Microsoft.EntityFrameworkCore;
using OCR.Domain.Entities;
using OCR.Domain.Interfaces;
using OCR.Infrastructure.Data;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Infrastructure.Repositories
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

        // TODO: Update this method after Patient model migration is complete
        // This method uses deprecated RecognizeText.FirstName/LastName which have been moved to Patient model
        /*
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
        */

        public async Task<RecognizeText> UpdateAsync(Guid id, RecognizeText textDomainModel)
        {
            var existingText = await dbContext.RecognizedDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (existingText == null)
            {
                return null;
            }
            
            // Update only medical data (patient data is now in Patient table)
            existingText.Medicine = textDomainModel.Medicine;
            existingText.Treatment = textDomainModel.Treatment;
            existingText.Examination = textDomainModel.Examination;
            existingText.ContraindicatedMedicine = textDomainModel.ContraindicatedMedicine;
            existingText.ContraindicatedReason = textDomainModel.ContraindicatedReason;
            existingText.DateDocument = textDomainModel.DateDocument;
            
            await dbContext.SaveChangesAsync();
            return existingText;
        }
    }
}
