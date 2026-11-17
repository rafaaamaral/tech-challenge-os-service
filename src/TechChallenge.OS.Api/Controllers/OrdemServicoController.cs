using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Api.Controllers;

/// <summary>
/// API REST para Ordens de Serviço.
/// 
/// Endpoints:
/// - POST /api/v1/ordens-servico — Criar nova OS
/// - GET /api/v1/ordens-servico — Listar todas as OSs
/// - GET /api/v1/ordens-servico/{id} — Obter OS por ID
/// - GET /api/v1/ordens-servico/por-codigo/{uniqueCode} — Consultar status via UniqueCode
/// - PATCH /api/v1/ordens-servico/{id}/entrega — Registrar entrega
/// - DELETE /api/v1/ordens-servico/{id} — Deletar OS (exclusão lógica)
/// </summary>
[ApiController]
[Route("api/v1/ordens-servico")]
[Produces("application/json")]
[Authorize]
public class OrdemServicoController : ControllerBase
{
    private readonly IOrdemServicoApplicationService _applicationService;
    private readonly ILogger<OrdemServicoController> _logger;

    public OrdemServicoController(
        IOrdemServicoApplicationService applicationService,
        ILogger<OrdemServicoController> logger)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria uma nova Ordem de Serviço.
    /// </summary>
    /// <param name="dto">Dados da Ordem de Serviço a ser criada</param>
    /// <returns>Ordem de Serviço criada com ID gerado</returns>
    /// <response code="201">Ordem de Serviço criada com sucesso</response>
    /// <response code="400">Dados inválidos ou Cliente/Veículo não encontrado</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrdemServicoResponseDto>> Criar(CreateOrdemServicoDto dto)
    {
        _logger.LogInformation("Criando nova Ordem de Serviço. Cliente: {ClienteId}, Veículo: {VeiculoId}",
            dto.ClienteId, dto.VeiculoId);

        var resultado = await _applicationService.CriarAsync(dto);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>
    /// Lista todas as Ordens de Serviço (ativas).
    /// </summary>
    /// <returns>Lista de Ordens de Serviço</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrdemServicoResponseDto>>> Listar()
    {
        _logger.LogInformation("Listando todas as Ordens de Serviço");

        var result = await _applicationService.ListarTodosAsync();

        return Ok(result);
    }

    /// <summary>
    /// Obtém uma Ordem de Serviço pelo ID.
    /// </summary>
    /// <param name="id">ID da Ordem de Serviço</param>
    /// <returns>Dados completos da Ordem de Serviço</returns>
    /// <response code="200">Ordem de Serviço retornada com sucesso</response>
    /// <response code="404">Ordem de Serviço não encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemServicoResponseDto>> ObterPorId(int id)
    {
        _logger.LogInformation("Obtendo Ordem de Serviço. ID: {Id}", id);

        var result = await _applicationService.ObterPorIdAsync(id);

        return Ok(result);
    }

    /// <summary>
    /// Consulta o status de uma Ordem de Serviço pelo UniqueCode.
    /// Útil para clientes que possuem apenas o código único de rastreamento.
    /// </summary>
    /// <param name="uniqueCode">Código único (Guid) da Ordem de Serviço</param>
    /// <returns>Dados completos da Ordem de Serviço</returns>
    /// <response code="200">Status retornado com sucesso</response>
    /// <response code="404">Ordem de Serviço não encontrada com o UniqueCode especificado</response>
    [HttpGet("por-codigo/{uniqueCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemServicoResponseDto>> ConsultarStatusPorCodigo(Guid uniqueCode)
    {
        _logger.LogInformation("Consultando status da Ordem de Serviço. UniqueCode: {UniqueCode}", uniqueCode);

        var result = await _applicationService.ConsultarStatusAsync(uniqueCode);

        return Ok(result);
    }

    /// <summary>
    /// Registra a entrega de uma Ordem de Serviço.
    /// Transição de estado: Finalizada → Entregue.
    /// </summary>
    /// <param name="id">ID da Ordem de Serviço a entregar</param>
    /// <returns>Dados atualizados da Ordem de Serviço</returns>
    /// <response code="200">Entrega registrada com sucesso</response>
    /// <response code="404">Ordem de Serviço não encontrada</response>
    /// <response code="409">Ordem de Serviço não pode ser entregue no estado atual</response>
    [HttpPatch("{id}/entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrdemServicoResponseDto>> Entregar(int id)
    {
        _logger.LogInformation("Registrando entrega da Ordem de Serviço. ID: {Id}", id);

        var result = await _applicationService.EntregarAsync(id);

        return Ok(result);
    }

    /// <summary>
    /// Deleta uma Ordem de Serviço (exclusão lógica).
    /// Marca a Ordem de Serviço como inativa sem apagar os dados do banco.
    /// </summary>
    /// <param name="id">ID da Ordem de Serviço a deletar</param>
    /// <response code="204">Ordem de Serviço deletada com sucesso</response>
    /// <response code="404">Ordem de Serviço não encontrada</response>
    /// <response code="409">Ordem de Serviço não pode ser deletada no estado atual</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Deletar(int id)
    {
        _logger.LogInformation("Deletando Ordem de Serviço. ID: {Id}", id);

        await _applicationService.DeletarAsync(id);

        return NoContent();
    }
}
