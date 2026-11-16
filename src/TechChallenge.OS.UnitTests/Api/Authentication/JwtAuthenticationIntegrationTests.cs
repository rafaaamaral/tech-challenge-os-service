using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace TechChallenge.OS.UnitTests.Api.Authentication;

public class JwtAuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "this-is-a-test-signing-key-with-min-32-chars";

    public JwtAuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:SigningKey"] = SigningKey,
                    ["ConnectionStrings:OSDb"] = "Host=localhost;Port=5433;Database=techchallenge_os_test;Username=postgres;Password=postgres"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Issuer,
                        ValidAudience = Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            });
        });
    }

    [Fact]
    public async Task OrdemServicoEndpoint_SemToken_DeveRetornar401()
    {
        var client = CreateHttpsClient();

        var response = await client.GetAsync("/api/v1/ordens-servico");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdemServicoEndpoint_TokenInvalido_DeveRetornar401()
    {
        var client = CreateHttpsClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token-invalido");

        var response = await client.GetAsync("/api/v1/ordens-servico");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdemServicoEndpoint_TokenExpirado_DeveRetornar401()
    {
        var client = CreateHttpsClient();
        var token = GenerateTokenFromRuntimeConfig(DateTime.UtcNow.AddMinutes(-5));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/ordens-servico");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdemServicoEndpoint_TokenValido_DevePassarAutenticacao()
    {
        var client = CreateHttpsClient();
        var token = GenerateTokenFromRuntimeConfig(DateTime.UtcNow.AddMinutes(30));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/ordens-servico");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_SemToken_DeveResponderSem401()
    {
        var client = CreateHttpsClient();

        var response = await client.GetAsync("/health");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void TokenValido_DeveConterClaimsDeIdentificacao()
    {
        var token = GenerateTokenFromRuntimeConfig(DateTime.UtcNow.AddMinutes(30));
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == "usuario.teste");
    }

    private string GenerateTokenFromRuntimeConfig(DateTime expiresAt)
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var issuer = configuration["Jwt:Issuer"] ?? Issuer;
        var audience = configuration["Jwt:Audience"] ?? Audience;
        var signingKey = configuration["Jwt:SigningKey"] ?? SigningKey;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "usuario.teste")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }
}
