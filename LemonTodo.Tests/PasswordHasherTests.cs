using LemonTodo.Server.Services;

namespace LemonTodo.Tests;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldGenerateValidHash()
    {
        var password = "TestPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
    {
        var password = "TestPassword123!";
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        var password = "TestPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForInvalidHash()
    {
        var password = "TestPassword123!";
        var invalidHash = "InvalidHash";

        var result = _passwordHasher.VerifyPassword(password, invalidHash);

        Assert.False(result);
    }
}
