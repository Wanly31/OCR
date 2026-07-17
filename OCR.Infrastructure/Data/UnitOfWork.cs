using OCR.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OCRDbContext _dbContext;

        public UnitOfWork(OCRDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _dbContext.SaveChangesAsync(ct);
        }
    }
}
