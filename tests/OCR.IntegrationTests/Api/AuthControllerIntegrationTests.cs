using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace OCR.IntegrationTests.Api;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
   
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    [Fact]
    public async Task Login_Should_ReturnToken_When_CredentialsAreValid()
    {
        var client = _factory.CreateClient();

        var loginRequest = new
        {
            Username = "kyrchul@gmail.com",
            Password = "kyrchul23"

        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/Auth/Login", content);
       

        response.StatusCode.Should().Be(HttpStatusCode.OK);


        var responseBody = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, JsonOptions);

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();

    }
}
public class LoginResponse
{
    [JsonPropertyName("_jwtToken")]
    public string Token { get; set; } = string.Empty;
}