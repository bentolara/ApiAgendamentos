using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Validators;

public class CriarAgendamentoDtoValidatorTests
{
    private readonly CriarAgendamentoDtoValidator _validator;

    public CriarAgendamentoDtoValidatorTests()
    {
        _validator = new CriarAgendamentoDtoValidator();
    }

    [Fact]
    public void Deve_Falhar_Quando_ClienteNome_Vazio()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = string.Empty,
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteNome);
    }

    [Fact]
    public void Deve_Falhar_Quando_ClienteTelefone_Vazio()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = string.Empty,
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteTelefone);
    }

    [Fact]
    public void Deve_Falhar_Quando_DataHoraInicio_Passada()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(-1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataHoraInicio);
    }

    [Fact]
    public void Deve_Falhar_Quando_DataHoraFim_Anterior_A_DataHoraInicio()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1).AddHours(2),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataHoraFim);
    }

    [Fact]
    public void Deve_Passar_Quando_Dados_Validos()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1),
            Observacoes = "Observação teste"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

