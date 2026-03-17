using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCR.Application.Abstractions;
using OCR.Infrastructure.Data;
using OCR.Infrastructure.Repositories;
using OCR.Infrastructure.Services;

namespace OCR.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<OCRAuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OCRAuthConnectionString")));
            services.AddDbContext<OCRDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OCRConnectionString")));

            services.AddScoped<ITokenService, TokenRepository>();
            services.AddScoped<IDocumentRepository, LocalDocumentRepository>();
            services.AddScoped<IRecognizeTextRepository, LocalRecognizeTextRepository>();
            services.AddScoped<IRecognizeRepository, LocalRecognizeRepository>();
            services.AddScoped<IPatientRepository, LocalPatientRepository>();
            services.AddScoped<IOcrProvider, AzureOcrService>();
            services.AddScoped<IMedicalExtractionService, AzureMedicalTextExtractionService>();
            services.AddScoped<IFileStorage, LocalFileStorageService>();

            services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("Course")
                .AddEntityFrameworkStores<OCRAuthDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            services.AddScoped<AzureOcrService>();
            services.AddScoped<RecognizeTextService>();


            return services;
        }
    }
}
