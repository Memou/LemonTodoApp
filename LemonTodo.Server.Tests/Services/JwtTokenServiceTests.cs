using FluentAssertions;
using LemonTodo.Server.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace LemonTodo.Server.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly1234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtTokenService = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidInputs_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtTokenService.GenerateToken(userId, username);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ValidInputs_TokenContainsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtTokenService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // JWT uses "sub" for NameIdentifier and "unique_name" for Name in some implementations
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.NameIdentifier || 
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == "nameid");

        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.Name || 
            c.Type == "unique_name");

        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(userId.ToString());

        usernameClaim.Should().NotBeNull();
        usernameClaim!.Value.Should().Be(username);
    }

    [Fact]
    public void GenerateToken_ValidInputs_TokenContainsCorrectIssuerAndAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtTokenService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateToken_ValidInputs_TokenExpiresIn7Days()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtTokenService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddDays(7);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ReturnsDifferentTokens()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var username1 = "testuser1";
        var username2 = "testuser2";

        // Act
        var token1 = _jwtTokenService.GenerateToken(userId1, username1);
        var token2 = _jwtTokenService.GenerateToken(userId2, username2);

        // Assert
        token1.Should().NotBe(token2);
    }
}
