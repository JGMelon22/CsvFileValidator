using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvFileValidator.Models;
using CsvFileValidator.Models.Enums;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvFileValidator.Infrastructure.Services;

public class CsvValidatorService
{
    public List<string> ValidateCsv(string filePath)
    {
        List<string> errors = new();

        if (Path.GetExtension(filePath).ToLower() != ".csv")
        {
            errors.Add("Apenas arquivos CSV são permitidos.");
            return errors;
        }

        if (!File.Exists(filePath))
        {
            errors.Add("Arquivo não encontrado.");
            return errors;
        }

        try
        {
            using StreamReader streamReader = new(filePath, Encoding.UTF8);
            CsvConfiguration csvConfiguiration = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using CsvReader csvReader = new(streamReader, csvConfiguiration);
            csvReader.Read();
            csvReader.ReadHeader();
            ValidateHeaders(csvReader.HeaderRecord!, errors);

            while (csvReader.Read())
            {
                List<string> rowErrors = new();
                ContactImport contact = MapContact(csvReader);

                rowErrors.AddRange(ValidateName(contact.FirstName, "Firstname"));
                rowErrors.AddRange(ValidateName(contact.LastName, "Lastname"));
                rowErrors.AddRange(ValidateEmail(contact.Email));
                rowErrors.AddRange(ValidateCpf(contact.Cpf));
                rowErrors.AddRange(ValidatePhone(contact.Phone));
                rowErrors.AddRange(ValidateGender(contact.Gender));
                rowErrors.AddRange(ValidateBirthday(contact.Birthday));

                if (rowErrors.Any())
                    errors.Add($"Linha {csvReader.Context.Parser!.Row}: " + string.Join("; ", rowErrors));
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Erro durante a validação: {ex.Message}");
        }

        return errors;
    }

    private void ValidateHeaders(string[] headers, List<string> errors)
    {
        string[] requiredHeaders = { "firstname", "lastname", "email", "cpf", "phone", "gender", "birthDay" };

        foreach (string header in requiredHeaders)
        {
            if (!headers.Contains(header))
            {
                errors.Add($"Cabeçalho obrigatório '{header}' não encontrado.");
            }
        }
    }

    private ContactImport MapContact(CsvReader csv)
    {
        return new ContactImport
        {
            FirstName = csv.GetField<string>("firstname")!,
            LastName = csv.GetField<string>("lastname")!,
            Email = csv.GetField<string>("email")!,
            Cpf = csv.GetField<string>("cpf")!,
            Phone = AdjustPhone(csv.GetField<string>("phone")!),
            Gender = (Gender?)MapGender(csv.GetField<string>("gender")!),
            Birthday = ParseDate(csv.GetField<string>("birthDay")!)
        };
    }

    private static string AdjustPhone(string phone)
    {
        string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

        if (!digitsOnly.StartsWith("55"))
            digitsOnly = "55" + digitsOnly;

        return digitsOnly;
    }

    private static Gender? MapGender(string genderValue)
    {
        return genderValue.ToUpper() switch
        {
            "FEMININO" => Gender.Feminino,
            "MASCULINO" => Gender.Masculino,
            _ => (Gender?)(-1)
        };
    }

    private static DateTime? ParseDate(string dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
            return null;

        if (DateTime.TryParseExact(dateValue, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return null;
    }

    private static List<string> ValidateName(string value, string fieldName)
    {
        return !string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, @"^[\p{L}]+$")
            ? new List<string> { $"{fieldName} deve conter apenas letras." }
            : [];
    }

    private static List<string> ValidateEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
        ? new List<string> { "Email deve estar em um formato válido." }
        : [];
    }

    private static List<string> ValidateCpf(string cpf)
    {
        return !string.IsNullOrWhiteSpace(cpf) && !Regex.IsMatch(cpf, @"^\d{11}$")
            ? new List<string> { "CPF deve conter apenas números e ter 11 dígitos." }
            : [];
    }

    private static List<string> ValidatePhone(string phone)
    {
        string phoneWithoutPrefix = phone;
        if (phone.StartsWith("55") && phone.Length >= 3)
            phoneWithoutPrefix = phone.Substring(2);

        return !string.IsNullOrWhiteSpace(phoneWithoutPrefix) &&
                   !Regex.IsMatch(phoneWithoutPrefix, @"^\d{10,11}$")
                ? new List<string> { "Telefone deve conter 10 ou 11 dígitos, incluindo o DDD (após o código do país 55)." }
                : [];
    }

    private static List<string> ValidateGender(Gender? gender)
    {
        if (!gender.HasValue)
            return new List<string>();

        return gender != Gender.Feminino && gender != Gender.Masculino
            ? new List<string> { "Gênero deve ser 'FEMININO' ou 'MASCULINO'." }
            : [];
    }

    private static List<string> ValidateBirthday(DateTime? birthday)
    {
        return !birthday.HasValue
        ? new List<string> { "Data de nascimento deve estar no formato válido de data." }
        : [];
    }
}
