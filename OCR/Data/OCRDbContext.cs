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
    }
}
