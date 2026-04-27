using Microsoft.Extensions.FileProviders;
using Serilog;
using OCR.Infrastructure;
using OCR.Host.Extensions;
using OCR.Application;
using OCR.Middlewares;

var MyAllowSpecifiOrigin = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

var origins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecifiOrigin,
        policy =>
        {
            policy.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

//add Logger
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/Log.txt",
        rollingInterval: RollingInterval.Minute,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.UseSwaggerMiddleware();
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecifiOrigin);


app.UseAuthentication();
app.UseAuthorization();

var documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
if (!Directory.Exists(documentsPath))
{
    Directory.CreateDirectory(documentsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(documentsPath),
    RequestPath = "/Documents"
    //https://localhost:7242/Images/filename.jpg
});
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

await app.RunAsync();

public partial class Program { }
