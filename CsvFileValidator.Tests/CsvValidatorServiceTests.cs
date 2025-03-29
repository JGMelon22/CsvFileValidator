using CsvFileValidator.Infrastructure.Services;
using Shouldly;

namespace CsvFileValidator.Tests;

public class CsvValidatorServiceTests
{
    [Fact]
    public void Should_ReturnNoErrors_When_ValidCsvFileProvided()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "valid_sample_contacts.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldBeEmpty();
    }
    
    [Fact]
    public void Should_ReturnHeaderErrors_When_MissingRequiredHeaders()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "missing_required_headers.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain("Cabeçalho obrigatório 'lastname' não encontrado.");
    }

    [Fact]
    public void Should_ReturnEmailError_When_InvalidEmailFormat()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "invalid_email_format.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain(error => error.Contains("Email deve estar em um formato válido."));
    }

    [Fact]
    public void Should_ReturnCpfError_When_InvalidCpfFormat()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "invalid_cpf.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain(error => error.Contains("CPF deve conter apenas números e ter 11 dígitos."));
    }

    // Test for invalid phone number length
    [Fact]
    public void Should_ReturnPhoneError_When_InvalidPhoneNumberLength()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "invalid_phone_number.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain(error => error.Contains("Telefone deve conter 10 ou 11 dígitos, incluindo o DDD (após o código do país 55)."));
    }

    // Test for invalid gender value
    [Fact]
    public void Should_ReturnGenderError_When_InvalidGenderValue()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "invalid_gender.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain(error => error.Contains("Gênero deve ser 'FEMININO' ou 'MASCULINO'."));
    }

    [Fact]
    public void Should_ReturnDateError_When_InvalidDateFormat()
    {
        // Arrange
        string filePath = Path.Combine("TestFiles", "invalid_date_format.csv");

        // Act
        List<string> errors = new CsvValidatorService().ValidateCsv(filePath);

        // Assert
        errors.ShouldContain(error => error.Contains("Data de nascimento deve estar no formato válido de data."));
    }
}
