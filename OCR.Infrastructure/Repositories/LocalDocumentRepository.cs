using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR.Domain.Entities;
using OCR.Application.Abstractions;
using OCR.Infrastructure.Data;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Infrastructure.Repositories
{
    public class LocalDocumentRepository : IDocumentRepository
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly OCRDbContext dbContext;

        public LocalDocumentRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, OCRDbContext dbContext)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }

        public async Task<Document> DeleteAsync(Guid id)
        {
            var existingDocument = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
            if(existingDocument == null)
            {
                return null;
            }

            dbContext.Documents.Remove(existingDocument);
            await dbContext.SaveChangesAsync();
            return existingDocument;
        }

        public async Task<List<Document>> GetAllAsync()
        {
            var document = dbContext.Documents.AsQueryable();
            return await document.ToListAsync();
        }

        public async Task<Document> GetByIdAsync(Guid id)
        {
            return await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        }

      
        public async Task<Document> UpdateAsync(Guid id, Document documentDomainModel)
        {
            var existingDocument = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
            if (existingDocument == null)
            {
                return null;
            }
            existingDocument.FileDescription = documentDomainModel.FileDescription;
            existingDocument.FileName = documentDomainModel.FileName;
            await dbContext.SaveChangesAsync();
            return existingDocument;
        }

        public async Task<Document> Upload(Document document)
        {
            await dbContext.Documents.AddAsync(document);
            await dbContext.SaveChangesAsync();

            return document;
        }
    }
}
