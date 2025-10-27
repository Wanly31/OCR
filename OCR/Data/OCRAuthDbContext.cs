using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OCR.Data
{
    public class OCRAuthDbContext : IdentityDbContext
    {
        public OCRAuthDbContext(DbContextOptions<OCRAuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var readerRoleID = "cefaa237-22fd-43f4-8226-89f4a6dfbd85";
            var writerRoleId = "a5069271-61dc-498c-9a6a-6bf6751351e8";

            var roles =new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerRoleID,
                    Name = "Reader",
                    NormalizedName = "READER"
                },
                new IdentityRole
                {
                    Id = writerRoleId,
                    Name = "Writer",
                    NormalizedName = "WRITER"
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
