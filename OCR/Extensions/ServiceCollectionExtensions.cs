namespace OCR.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Зберігає PascalCase як у C# моделях
            });
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddAuth(configuration);
            services.AddSwagger();

            return services;
        }
    }
}
