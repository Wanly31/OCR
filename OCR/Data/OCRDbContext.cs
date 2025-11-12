using Microsoft.EntityFrameworkCore;
using OCR.Models.Domain;

namespace OCR.Data
{
    public class OCRDbContext : DbContext
    {
        public OCRDbContext(DbContextOptions<OCRDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<RecognizeText> RecognizedDocuments { get; set; }
        public DbSet<Recognize> RecognizedTexts { get; set; }
    }
}
