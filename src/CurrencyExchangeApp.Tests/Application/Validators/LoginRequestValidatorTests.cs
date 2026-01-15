using CurrencyExchangeApp.Application.Validators;
using CurrencyExchangeApp.Core.DTOs.Requests;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CurrencyExchangeApp.Tests.Application.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidCredentials_ShouldPass()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyEmail_ShouldFail(string? email)
    {
        // Arrange
        var request = new LoginRequest { Email = email!, Password = "Password123" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    public async Task Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var request = new LoginRequest { Email = email, Password = "Password123" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyPassword_ShouldFail(string? password)
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = password! };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }
}
