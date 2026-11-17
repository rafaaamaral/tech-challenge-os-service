using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Exceptions;
using TechChallenge.OS.Application.Interfaces.Repositories;
using TechChallenge.OS.Application.Interfaces.Services;
using TechChallenge.OS.Application.Mappers;
using TechChallenge.OS.Application.Services.Base;
using TechChallenge.OS.Application.Validators;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Domain.Exceptions;

namespace TechChallenge.OS.Application.Services.OrdemServicos
{
    /// <summary>
    /// Serviço de aplicação para os casos de uso de Ordem de Serviço.
    /// </summary>
    public class OrdemServicoApplicationService : ServiceBase<OrdemServicoApplicationService>, IOrdemServicoApplicationService
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository;
        private readonly IClienteService _clienteService;
        private readonly IVeiculoService _veiculoService;
        private readonly INotificacaoService _notificacaoService;

        public OrdemServicoApplicationService(
            IOrdemServicoRepository ordemServicoRepository,
            IClienteService clienteService,
            IVeiculoService veiculoService,
            INotificacaoService notificacaoService)
        {
            _ordemServicoRepository = ordemServicoRepository ?? throw new ArgumentNullException(nameof(ordemServicoRepository));
            _clienteService = clienteService ?? throw new ArgumentNullException(nameof(clienteService));
            _veiculoService = veiculoService ?? throw new ArgumentNullException(nameof(veiculoService));
            _notificacaoService = notificacaoService ?? throw new ArgumentNullException(nameof(notificacaoService));
        }

        public async Task<OrdemServicoResponseDto> CriarAsync(CreateOrdemServicoDto dto)
        {
            OrdemServicoValidator.ValidarCreateOrdemServicoDto(dto);

            var clienteExiste = await _clienteService.ClienteExisteEstaAtivoAsync(dto.ClienteId);
            if (!clienteExiste)
                throw new NotFoundException("Cliente", dto.ClienteId);

            var veiculoPertence = await _veiculoService.VeiculoPertenceAoClienteAsync(dto.VeiculoId, dto.ClienteId);
            if (!veiculoPertence)
                throw new System.InvalidOperationException(
                    $"O veículo {dto.VeiculoId} não existe ou não pertence ao cliente {dto.ClienteId}.");

            var proximoNumero = await ObterProximoNumeroAsync();
            var ordemServico = OrdemServico.Criar(dto.ClienteId, dto.VeiculoId, proximoNumero);

            foreach (var itemServico in dto.ItensServicos)
            {
                OrdemServicoValidator.ValidarCreateOrdemServicoItemServicoDto(itemServico);
                var descricao = itemServico.Descricao ?? $"Serviço {itemServico.ServicoId}";
                ordemServico.AdicionarServico(itemServico.ServicoId, descricao);
            }

            foreach (var itemPeca in dto.ItensPecasInsumos)
            {
                OrdemServicoValidator.ValidarCreateOrdemServicoItemPecaInsumoDto(itemPeca);
                var descricao = itemPeca.Descricao ?? $"Peça/Insumo {itemPeca.PecaInsumoId}";
                ordemServico.AdicionarPecaInsumo(itemPeca.PecaInsumoId, descricao, itemPeca.Quantidade);
            }

            var gravada = await _ordemServicoRepository.AddAsync(ordemServico);
            return gravada.ToResponseDto();
        }

        public async Task<List<OrdemServicoResponseDto>> ListarTodosAsync()
        {
            var ordens = await _ordemServicoRepository.ListAllWithItemsAsync();
            return ordens.ToResponseDtoList();
        }

        public async Task<OrdemServicoResponseDto> ObterPorIdAsync(int id)
        {
            var ordemServico = await _ordemServicoRepository.GetByIdWithItemsAsync(id)
                ?? throw new NotFoundException("Ordem de Serviço", id);

            return ordemServico.ToResponseDto();
        }

        public async Task<OrdemServicoResponseDto> ConsultarStatusAsync(Guid uniqueCode)
        {
            var ordemServico = await _ordemServicoRepository.GetByUniqueCodeAsync(uniqueCode)
                ?? throw new NotFoundException("Ordem de Serviço", uniqueCode);

            return ordemServico.ToResponseDto();
        }

        public async Task<OrdemServicoResponseDto> EntregarAsync(int id)
        {
            var ordemServico = await ObterOuFalharAsync(id);

            try { ordemServico.Entregar(); }
            catch (DomainException ex)
            {
                throw new System.InvalidOperationException($"Não foi possível entregar: {ex.Message}", ex);
            }

            await _ordemServicoRepository.UpdateAsync(ordemServico);

            _ = Task.Run(async () =>
            {
                try
                {
                    var cliente = await _clienteService.ObterInfoClienteAsync(ordemServico.ClienteId);
                    var email = cliente?.Email;
                    if (!string.IsNullOrWhiteSpace(email))
                        await _notificacaoService.EnviarConfirmacaoEntregaAsync(email, ordemServico.Numero);
                }
                catch
                {
                }
            });

            return await ObterPorIdAsync(id);
        }

        public async Task DeletarAsync(int id)
        {
            var ordemServico = await ObterOuFalharAsync(id);

            try { ordemServico.Delete(); }
            catch (DomainException ex)
            {
                throw new System.InvalidOperationException($"Não foi possível deletar: {ex.Message}", ex);
            }

            await _ordemServicoRepository.UpdateAsync(ordemServico);
        }

        private async Task<int> ObterProximoNumeroAsync()
        {
            return await _ordemServicoRepository.GetNextNumeroAsync();
        }

        private async Task<OrdemServico> ObterOuFalharAsync(int id)
        {
            var ordemServico = await _ordemServicoRepository.GetByIdAsync(id);
            if (ordemServico == null)
                throw new NotFoundException("Ordem de Serviço", id);

            return ordemServico;
        }
    }
}
