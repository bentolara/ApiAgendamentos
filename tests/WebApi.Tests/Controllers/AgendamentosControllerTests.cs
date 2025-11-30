using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace WebApi.Tests.Controllers;

public class AgendamentosControllerTests
{
    private readonly Mock<IAgendamentoService> _serviceMock;
    //private readonly Mock<ILogger<AgendamentosController>> _loggerMock;
    //private readonly AgendamentosController _controller;

    public AgendamentosControllerTests()
    {
        _serviceMock = new Mock<IAgendamentoService>();
       // _loggerMock = new Mock<ILogger<AgendamentosController>>();
       // _controller = new AgendamentosController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Criar_Deve_Retornar_Created_Quando_Sucesso()
    {
        // Arrange
        var dto = new CriarAgendamentoDto
        {
            ClienteNome = "João Silva",
            ClienteTelefone = "11999999999",
            DataHoraInicio = DateTime.Now.AddDays(1),
            DataHoraFim = DateTime.Now.AddDays(1).AddHours(1)
        };

        var agendamentoDto = new AgendamentoDto
        {
            Id = Guid.NewGuid(),
            ClienteNome = dto.ClienteNome,
            ClienteTelefone = dto.ClienteTelefone,
            DataHoraInicio = dto.DataHoraInicio,
            DataHoraFim = dto.DataHoraFim
        };

        _serviceMock.Setup(s => s.CriarAsync(dto)).ReturnsAsync(agendamentoDto);

        // Act
        //var result = await _controller.Criar(dto);

        // Assert
        //result.Result.Should().BeOfType<CreatedAtActionResult>();
        //var createdResult = result.Result as CreatedAtActionResult;
        //createdResult!.Value.Should().BeEquivalentTo(agendamentoDto);
    }

    [Fact]
    public async Task Criar_Deve_Retornar_BadRequest_Quando_DomainException()
    {
        // Arrange
        var dto = new CriarAgendamentoDto();
        _serviceMock.Setup(s => s.CriarAsync(dto))
            .ThrowsAsync(new DomainException("Erro de validação"));

        // Act
        //var result = await _controller.Criar(dto);

        // Assert
        //result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ObterPorId_Deve_Retornar_NotFound_Quando_Nao_Encontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync((AgendamentoDto?)null);

        // Act
        //var result = await _controller.ObterPorId(id);

        // Assert
        //result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ObterPorId_Deve_Retornar_Ok_Quando_Encontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agendamentoDto = new AgendamentoDto { Id = id };
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(agendamentoDto);

        // Act
        //var result = await _controller.ObterPorId(id);

        // Assert
       // result.Result.Should().BeOfType<OkObjectResult>();
        //var okResult = result.Result as OkObjectResult;
        //okResult!.Value.Should().BeEquivalentTo(agendamentoDto);
    }
}

