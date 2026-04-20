using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace OCR.IntegrationTests.Api;

public class OcrControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OcrControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadAndRecognize_Should_Return_Ok()
    {
        var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("fake file content");
        content.Add(new ByteArrayContent(fileBytes), "File", "test.pdf");
        content.Add(new StringContent("Test upload"), "Description");

        var response = await _client.PostAsync("/api/Ocr/UploadAndRecognize", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
