using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechChallenge.OS.Api.Controllers;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Exceptions;
using TechChallenge.OS.Application.Interfaces.Services;
using TechChallenge.OS.Domain.Common.Enums;
using Xunit;

namespace TechChallenge.OS.UnitTests.Api.Controllers;

/// <summary>
/// Testes para OrdemServicoController.
/// Valida endpoints REST e tratamento de erros.
/// </summary>
public class OrdemServicoControllerTests
{
    private readonly Mock<IOrdemServicoApplicationService> _mockApplicationService;
    private readonly Mock<ILogger<OrdemServicoController>> _mockLogger;
    private readonly OrdemServicoController _controller;

    public OrdemServicoControllerTests()
    {
        _mockApplicationService = new Mock<IOrdemServicoApplicationService>();
        _mockLogger = new Mock<ILogger<OrdemServicoController>>();
        _controller = new OrdemServicoController(_mockApplicationService.Object, _mockLogger.Object);
    }

    #region POST /api/v1/ordens-servico

    [Fact]
    public async Task Criar_ComDadosValidos_Retorna201Created()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto
        {
            ClienteId = 1,
            VeiculoId = 1,
            ItensServicos = new List<CreateOrdemServicoItemServicoDto>
            {
                new() { ServicoId = 1, Descricao = "Revisão" }
            },
            ItensPecasInsumos = new()
        };

        var responseDto = new OrdemServicoResponseDto
        {
            Id = 1,
            UniqueCode = Guid.NewGuid(),
            ClienteId = 1,
            VeiculoId = 1,
            Numero = 1,
            Status = StatusOrdemServico.Recebida,
            ItensServicos = new(),
            ItensPecasInsumos = new()
        };

        _mockApplicationService
            .Setup(x => x.CriarAsync(It.IsAny<CreateOrdemServicoDto>()))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.Criar(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.ObterPorId), createdResult.ActionName);
        Assert.Equal(responseDto.Id, ((OrdemServicoResponseDto)createdResult.Value!).Id);
        _mockApplicationService.Verify(x => x.CriarAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Criar_ComClienteInvalido_Retorna400BadRequest()
    {
        // Arrange
        var createDto = new CreateOrdemServicoDto
        {
            ClienteId = 999,
            VeiculoId = 1
        };

        _mockApplicationService
            .Setup(x => x.CriarAsync(It.IsAny<CreateOrdemServicoDto>()))
            .ThrowsAsync(new NotFoundException("Cliente", 999));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Criar(createDto));
    }

    #endregion

    #region GET /api/v1/ordens-servico

    [Fact]
    public async Task Listar_RetornaListaDOrdensComSucesso()
    {
        // Arrange
        var ordensDto = new List<OrdemServicoResponseDto>
        {
            new()
            {
                Id = 1,
                UniqueCode = Guid.NewGuid(),
                ClienteId = 1,
                VeiculoId = 1,
                Numero = 1,
                Status = StatusOrdemServico.Recebida
            },
            new()
            {
                Id = 2,
                UniqueCode = Guid.NewGuid(),
                ClienteId = 2,
                VeiculoId = 2,
                Numero = 2,
                Status = StatusOrdemServico.Recebida
            }
        };

        _mockApplicationService
            .Setup(x => x.ListarTodosAsync())
            .ReturnsAsync(ordensDto);

        // Act
        var result = await _controller.Listar();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<OrdemServicoResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
        _mockApplicationService.Verify(x => x.ListarTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task Listar_RetornaListaVaziaQuandoNaoHaOrdensa()
    {
        // Arrange
        _mockApplicationService
            .Setup(x => x.ListarTodosAsync())
            .ReturnsAsync(new List<OrdemServicoResponseDto>());

        // Act
        var result = await _controller.Listar();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<OrdemServicoResponseDto>>(okResult.Value);
        Assert.Empty(returnedList);
    }

    #endregion

    #region GET /api/v1/ordens-servico/{id}

    [Fact]
    public async Task ObterPorId_ComIdValido_Retorna200OK()
    {
        // Arrange
        var ordemId = 1;
        var responseDto = new OrdemServicoResponseDto
        {
            Id = ordemId,
            UniqueCode = Guid.NewGuid(),
            ClienteId = 1,
            VeiculoId = 1,
            Numero = 1,
            Status = StatusOrdemServico.Recebida
        };

        _mockApplicationService
            .Setup(x => x.ObterPorIdAsync(ordemId))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.ObterPorId(ordemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);
        Assert.Equal(ordemId, returnedOrder.Id);
        _mockApplicationService.Verify(x => x.ObterPorIdAsync(ordemId), Times.Once);
    }

    [Fact]
    public async Task ObterPorId_ComIdInvalido_Retorna404NotFound()
    {
        // Arrange
        var ordemId = 999;
        _mockApplicationService
            .Setup(x => x.ObterPorIdAsync(ordemId))
            .ThrowsAsync(new NotFoundException("Ordem de Serviço", ordemId));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.ObterPorId(ordemId));
    }

    #endregion

    #region GET /api/v1/ordens-servico/por-codigo/{uniqueCode}

    [Fact]
    public async Task ConsultarStatusPorCodigo_ComCodigoValido_Retorna200OK()
    {
        // Arrange
        var uniqueCode = Guid.NewGuid();
        var responseDto = new OrdemServicoResponseDto
        {
            Id = 1,
            UniqueCode = uniqueCode,
            ClienteId = 1,
            VeiculoId = 1,
            Numero = 1,
            Status = StatusOrdemServico.Recebida
        };

        _mockApplicationService
            .Setup(x => x.ConsultarStatusAsync(uniqueCode))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.ConsultarStatusPorCodigo(uniqueCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);
        Assert.Equal(uniqueCode, returnedOrder.UniqueCode);
        _mockApplicationService.Verify(x => x.ConsultarStatusAsync(uniqueCode), Times.Once);
    }

    [Fact]
    public async Task ConsultarStatusPorCodigo_ComCodigoInvalido_Retorna404NotFound()
    {
        // Arrange
        var invalidCode = Guid.NewGuid();
        _mockApplicationService
            .Setup(x => x.ConsultarStatusAsync(invalidCode))
            .ThrowsAsync(new NotFoundException("Ordem de Serviço", invalidCode));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.ConsultarStatusPorCodigo(invalidCode));
    }

    #endregion

    #region PATCH /api/v1/ordens-servico/{id}/entrega

    [Fact]
    public async Task Entregar_ComOrdemValida_Retorna200OK()
    {
        // Arrange
        var ordemId = 1;
        var responseDto = new OrdemServicoResponseDto
        {
            Id = ordemId,
            UniqueCode = Guid.NewGuid(),
            ClienteId = 1,
            VeiculoId = 1,
            Numero = 1,
            Status = StatusOrdemServico.Entregue
        };

        _mockApplicationService
            .Setup(x => x.EntregarAsync(ordemId))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.Entregar(ordemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);
        Assert.Equal(StatusOrdemServico.Entregue, returnedOrder.Status);
        _mockApplicationService.Verify(x => x.EntregarAsync(ordemId), Times.Once);
    }

    [Fact]
    public async Task Entregar_ComOrdemNaoEncontrada_Retorna404NotFound()
    {
        // Arrange
        var ordemId = 999;
        _mockApplicationService
            .Setup(x => x.EntregarAsync(ordemId))
            .ThrowsAsync(new NotFoundException("Ordem de Serviço", ordemId));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Entregar(ordemId));
    }

    [Fact]
    public async Task Entregar_ComOrdemEmEstadoInvalido_Retorna409Conflict()
    {
        // Arrange
        var ordemId = 1;
        _mockApplicationService
            .Setup(x => x.EntregarAsync(ordemId))
            .ThrowsAsync(new InvalidOperationException("Não foi possível entregar: Ordem de Serviço em estado inválido"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Entregar(ordemId));
    }

    #endregion

    #region DELETE /api/v1/ordens-servico/{id}

    [Fact]
    public async Task Deletar_ComOrdemValida_Retorna204NoContent()
    {
        // Arrange
        var ordemId = 1;
        _mockApplicationService
            .Setup(x => x.DeletarAsync(ordemId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Deletar(ordemId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockApplicationService.Verify(x => x.DeletarAsync(ordemId), Times.Once);
    }

    [Fact]
    public async Task Deletar_ComOrdemNaoEncontrada_Retorna404NotFound()
    {
        // Arrange
        var ordemId = 999;
        _mockApplicationService
            .Setup(x => x.DeletarAsync(ordemId))
            .ThrowsAsync(new NotFoundException("Ordem de Serviço", ordemId));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Deletar(ordemId));
    }

    [Fact]
    public async Task Deletar_ComOrdemEmEstadoInvalido_Retorna409Conflict()
    {
        // Arrange
        var ordemId = 1;
        _mockApplicationService
            .Setup(x => x.DeletarAsync(ordemId))
            .ThrowsAsync(new InvalidOperationException("Não foi possível deletar: Ordem de Serviço em estado inválido"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Deletar(ordemId));
    }

    #endregion
}
