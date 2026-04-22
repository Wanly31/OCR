using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OCR.Application.Abstractions;
using OCR.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace OCR.IntegrationTests.Api;

public class OcrControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public OcrControllerIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                                
                var mockOcr = new Mock<IOcrProvider>();
                mockOcr.Setup(x => x.RecognizeTextFromFileAsync(It.IsAny<string>()))
                       .ReturnsAsync("Mocked extracted text");

                var mockExtraction = new Mock<IMedicalExtractionService>();
                mockExtraction.Setup(x => x.ExtractMedicalDataAsync(It.IsAny<string>()))
                              .ReturnsAsync(new RecognizedTextResultDto
                              {
                                  FirstName = "TestFirst",
                                  LastName = "TestLast"
                              });

                services.AddScoped(_ => mockOcr.Object);
                services.AddScoped(_ => mockExtraction.Object);
            });
        }).CreateClient();
        _output = output;
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private async Task<string> GetTokenAsync()
    {
        var loginRequest = new
        {
            Username = "kyrchul@gmail.com",
            Password = "kyrchul23"

        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/Auth/Login", content);


        response.StatusCode.Should().Be(HttpStatusCode.OK);


        var responseBody = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, JsonOptions);
        return result!.Token;
    }

    [Fact]
    public async Task UploadAndRecognize_Should_Return_Ok()
    {
        var token = await GetTokenAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();

        var fileBytes = Encoding.UTF8.GetBytes("fake file content");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        content.Add(fileContent, "File", "test.pdf");
        content.Add(new StringContent("test.pdf"), "FileName");
        content.Add(new StringContent("Test upload"), "FileDescription");

        var response = await _client.PostAsync("/api/Ocr/UploadAndRecognize", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        _output.WriteLine(responseBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public class LoginResponse
    {
        [JsonPropertyName("_jwtToken")]
        public string Token { get; set; } = string.Empty;
    }
}
