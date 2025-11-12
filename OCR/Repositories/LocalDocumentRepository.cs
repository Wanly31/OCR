using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Repositories
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

        public async Task<List<Document>> GetAllAsync()
        {
            var document = dbContext.Documents.AsQueryable();
            return await document.ToListAsync();
        }

        public async Task<Document> GetByIdAsync(Guid id)
        {
            return await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Document> Upload(Document document)
        {
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Documents",
               $"{document.FileName}{document.FileExtension}");

            //upload file to local path
            using var stream = new FileStream(localFilePath, FileMode.Create);
            await document.File.CopyToAsync(stream);

            //https://localhost:123/images/images.jpg

            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Documents/{document.FileName}{document.FileExtension}";

            document.FilePath = urlFilePath;

            //add image to database
            await dbContext.Documents.AddAsync(document);
            await dbContext.SaveChangesAsync();

            return document;
        }
    }
}
