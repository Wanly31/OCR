using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OCR.Application.Features.Auth.LoginUser;
using System.Net;
using System.Text;
using System.Text.Json;
using static OCR.IntegrationTests.Api.OcrControllerIntegrationTests;

namespace OCR.IntegrationTests.Api;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMediator));
                if (descriptor != null)
                    services.Remove(descriptor);

                var mockMediator = new Mock<IMediator>();

                mockMediator
                    .Setup(m => m.Send(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new LoginUserResult ("fake-jwt-token" ));

                services.AddScoped(_ => mockMediator.Object);
            });
        });
    }

    [Fact]
    public async Task Login_Should_ReturnToken_When_CredentialsAreValid()
    {
        var client = _factory.CreateClient();

        var loginRequest = new
        {
            Username = "test@example.com",
            Password = "TestPassword123!"
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