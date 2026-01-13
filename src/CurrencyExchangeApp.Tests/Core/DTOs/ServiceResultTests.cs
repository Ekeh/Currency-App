using CurrencyExchangeApp.Core.DTOs;

namespace CurrencyExchangeApp.Tests.Core.DTOs;

public class ServiceResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Arrange
        var data = "test data";

        // Act
        var result = ServiceResult<string>.Success(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(data);
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = ServiceResult<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationFailure_CreatesValidationFailureResult()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = ServiceResult<string>.ValidationFailure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ValidationErrors_DefaultsToEmptyList()
    {
        // Arrange & Act
        var result = new ServiceResult<string>();

        // Assert
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().BeEmpty();
    }
}
