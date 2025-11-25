using Application.DTOs;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AgendamentoServiceTests
{
    private readonly Mock<IAgendamentoRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AgendamentoService _service;

    public AgendamentoServiceTests()
    {
        _repositoryMock = new Mock<IAgendamentoRepository>();
        _mapperMock = new Mock<IMapper>();
        _service = new AgendamentoService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CriarAsync_Deve_Retornar_AgendamentoDto_Quando_Sucesso()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ClienteNome = dto.ClienteNome,
            ClienteTelefone = dto.ClienteTelefone,
            DataHoraInicio = dto.DataHoraInicio,
            DataHoraFim = dto.DataHoraFim
        };

        var agendamentoDto = new AgendamentoDto
        {
            Id = agendamento.Id,
            ClienteNome = agendamento.ClienteNome,
            ClienteTelefone = agendamento.ClienteTelefone,
            DataHoraInicio = agendamento.DataHoraInicio,
            DataHoraFim = agendamento.DataHoraFim
        };

        _repositoryMock.Setup(r => r.ExisteConflitoHorarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map<Agendamento>(dto)).Returns(agendamento);
        _mapperMock.Setup(m => m.Map<AgendamentoDto>(agendamento)).Returns(agendamentoDto);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Agendamento>())).ReturnsAsync(agendamento);

        // Act
        var result = await _service.CriarAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ClienteNome.Should().Be(dto.ClienteNome);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Agendamento>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_Deve_Lancar_Excecao_Quando_Conflito_Horario()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        var agendamento = new Agendamento();

        _repositoryMock.Setup(r => r.ExisteConflitoHorarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(true);

        _mapperMock.Setup(m => m.Map<Agendamento>(dto)).Returns(agendamento);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(dto));
    }

    [Fact]
    public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Nao_Encontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Agendamento?)null);

        // Act
        var result = await _service.ObterPorIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeletarAsync_Deve_Lancar_Excecao_Quando_Nao_Encontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.DeletarAsync(id));
    }
}

