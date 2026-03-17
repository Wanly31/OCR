using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using OCR.Application.Abstractions;
using OCR.Infrastructure.Repositories;
using OCR.Infrastructure.Data;
using OCR.Infrastructure.Services;
using Serilog;
using System.Text;
using OCR.Infrastructure;
using OCR.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseSwaggerMiddleware();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Documents")),
    RequestPath = "/Documents"
    //https://localhost:7242/Images/filename.jpg
});

app.MapControllers();

await app.RunAsync();

