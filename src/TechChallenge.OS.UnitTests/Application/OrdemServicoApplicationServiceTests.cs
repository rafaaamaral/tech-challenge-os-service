using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Exceptions;
using TechChallenge.OS.Application.Interfaces.Repositories;
using TechChallenge.OS.Application.Interfaces.Services;
using TechChallenge.OS.Application.Services.OrdemServicos;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Domain.Common.Enums;

namespace TechChallenge.OS.UnitTests.Application;

public class OrdemServicoApplicationServiceTests
{
    [Fact]
    public async Task CriarAsync_DeveCriarOS_QuandoDadosValidos()
    {
        var repo = new FakeOrdemServicoRepository();
        var cliente = new FakeClienteService { ClienteExiste = true };
        var veiculo = new FakeVeiculoService { VeiculoPertence = true };
        var notificacao = new FakeNotificacaoService();
        var service = new OrdemServicoApplicationService(repo, cliente, veiculo, notificacao);

        var dto = new CreateOrdemServicoDto
        {
            ClienteId = 1,
            VeiculoId = 2,
            ItensServicos = [new CreateOrdemServicoItemServicoDto { ServicoId = 10, Descricao = "Troca" }],
            ItensPecasInsumos = [new CreateOrdemServicoItemPecaInsumoDto { PecaInsumoId = 20, Descricao = "Filtro", Quantidade = 1 }]
        };

        var result = await service.CriarAsync(dto);

        Assert.Equal(1, result.ClienteId);
        Assert.Equal(2, result.VeiculoId);
        Assert.Single(result.ItensServicos);
        Assert.Single(result.ItensPecasInsumos);
        Assert.Equal(1, repo.AddCalls);
    }

    [Fact]
    public async Task CriarAsync_DeveFalhar_QuandoClienteInexistente()
    {
        var service = BuildService(clienteExiste: false, veiculoPertence: true);
        var dto = ValidCreateDto();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CriarAsync(dto));
    }

    [Fact]
    public async Task CriarAsync_DeveFalhar_QuandoVeiculoNaoPertence()
    {
        var service = BuildService(clienteExiste: true, veiculoPertence: false);
        var dto = ValidCreateDto();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(dto));
    }

    [Fact]
    public async Task CriarAsync_DeveFalhar_QuandoValidacaoInvalida()
    {
        var service = BuildService();
        var dto = new CreateOrdemServicoDto { ClienteId = 0, VeiculoId = 2 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(dto));
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarOS_QuandoExistente()
    {
        var repo = new FakeOrdemServicoRepository();
        var os = OrdemServico.Criar(1, 2, 10);
        await repo.AddAsync(os);

        var service = new OrdemServicoApplicationService(repo, new FakeClienteService(), new FakeVeiculoService(), new FakeNotificacaoService());

        var result = await service.ObterPorIdAsync(os.Id);

        Assert.Equal(os.UniqueCode, result.UniqueCode);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveFalhar_QuandoNaoEncontrada()
    {
        var service = BuildService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ObterPorIdAsync(999));
    }

    [Fact]
    public async Task ConsultarStatusAsync_DeveRetornarOS_QuandoExistente()
    {
        var repo = new FakeOrdemServicoRepository();
        var os = OrdemServico.Criar(1, 2, 10);
        await repo.AddAsync(os);

        var service = new OrdemServicoApplicationService(repo, new FakeClienteService(), new FakeVeiculoService(), new FakeNotificacaoService());

        var result = await service.ConsultarStatusAsync(os.UniqueCode);

        Assert.Equal(os.Id, result.Id);
    }

    [Fact]
    public async Task ConsultarStatusAsync_DeveFalhar_QuandoNaoEncontrada()
    {
        var service = BuildService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ConsultarStatusAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarTodosAsync_DeveRetornarLista()
    {
        var repo = new FakeOrdemServicoRepository();
        await repo.AddAsync(OrdemServico.Criar(1, 2, 10));
        await repo.AddAsync(OrdemServico.Criar(1, 3, 11));

        var service = new OrdemServicoApplicationService(repo, new FakeClienteService(), new FakeVeiculoService(), new FakeNotificacaoService());

        var list = await service.ListarTodosAsync();

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task EntregarAsync_DeveEntregarEAtualizar()
    {
        var repo = new FakeOrdemServicoRepository();
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarAguardandoAprovacaoOrcamento();
        os.RegistrarExecucaoIniciada();
        os.RegistrarExecucaoFinalizada();
        await repo.AddAsync(os);

        var cliente = new FakeClienteService { ClienteInfo = (1, "Cliente", "cliente@teste.com") };
        var notificacao = new FakeNotificacaoService();
        var service = new OrdemServicoApplicationService(repo, cliente, new FakeVeiculoService(), notificacao);

        var result = await service.EntregarAsync(os.Id);

        Assert.Equal(StatusOrdemServico.Entregue, result.Status);
        Assert.Equal(1, repo.UpdateCalls);

        await Task.Delay(50);
        Assert.True(notificacao.EnviarConfirmacaoCalls >= 1);
    }

    [Fact]
    public async Task EntregarAsync_DeveFalhar_QuandoTransicaoInvalida()
    {
        var repo = new FakeOrdemServicoRepository();
        var os = OrdemServico.Criar(1, 2, 10);
        await repo.AddAsync(os);

        var service = new OrdemServicoApplicationService(repo, new FakeClienteService(), new FakeVeiculoService(), new FakeNotificacaoService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EntregarAsync(os.Id));
    }

    [Fact]
    public async Task DeletarAsync_DeveRealizarExclusaoLogica()
    {
        var repo = new FakeOrdemServicoRepository();
        var os = OrdemServico.Criar(1, 2, 10);
        await repo.AddAsync(os);

        var service = new OrdemServicoApplicationService(repo, new FakeClienteService(), new FakeVeiculoService(), new FakeNotificacaoService());

        await service.DeletarAsync(os.Id);

        var persisted = await repo.GetByIdAsync(os.Id);
        Assert.NotNull(persisted);
        Assert.False(persisted!.Ativo);
        Assert.Equal(1, repo.UpdateCalls);
    }

    [Fact]
    public async Task DeletarAsync_DeveFalhar_QuandoNaoEncontrada()
    {
        var service = BuildService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeletarAsync(999));
    }

    private static OrdemServicoApplicationService BuildService(bool clienteExiste = true, bool veiculoPertence = true)
    {
        return new OrdemServicoApplicationService(
            new FakeOrdemServicoRepository(),
            new FakeClienteService { ClienteExiste = clienteExiste },
            new FakeVeiculoService { VeiculoPertence = veiculoPertence },
            new FakeNotificacaoService());
    }

    private static CreateOrdemServicoDto ValidCreateDto()
    {
        return new CreateOrdemServicoDto
        {
            ClienteId = 1,
            VeiculoId = 2,
            ItensServicos = [new CreateOrdemServicoItemServicoDto { ServicoId = 10, Descricao = "Troca" }],
            ItensPecasInsumos = [new CreateOrdemServicoItemPecaInsumoDto { PecaInsumoId = 20, Descricao = "Filtro", Quantidade = 1 }]
        };
    }

    private sealed class FakeOrdemServicoRepository : IOrdemServicoRepository
    {
        private readonly List<OrdemServico> _data = [];
        private int _idSeed = 1;

        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }

        public Task<OrdemServico> AddAsync(OrdemServico ordemServico)
        {
            ordemServico.Id = _idSeed++;
            _data.Add(ordemServico);
            AddCalls++;
            return Task.FromResult(ordemServico);
        }

        public Task<int> GetNextNumeroAsync() => Task.FromResult(_data.Count + 1);

        public Task DeleteAsync(int id)
        {
            var found = _data.FirstOrDefault(x => x.Id == id);
            if (found != null)
                _data.Remove(found);
            return Task.CompletedTask;
        }

        public Task<OrdemServico?> GetByIdAsync(int id)
            => Task.FromResult(_data.FirstOrDefault(x => x.Id == id));

        public Task<OrdemServico?> GetByIdWithItemsAsync(int id)
            => Task.FromResult(_data.FirstOrDefault(x => x.Id == id));

        public Task<OrdemServico?> GetByUniqueCodeAsync(Guid uniqueCode)
            => Task.FromResult(_data.FirstOrDefault(x => x.UniqueCode == uniqueCode));

        public Task<IEnumerable<OrdemServico>> ListAllWithItemsAsync()
            => Task.FromResult(_data.AsEnumerable());

        public Task<OrdemServico> UpdateAsync(OrdemServico ordemServico)
        {
            UpdateCalls++;
            return Task.FromResult(ordemServico);
        }
    }

    private sealed class FakeClienteService : IClienteService
    {
        public bool ClienteExiste { get; set; } = true;
        public (int Id, string Nome, string Email)? ClienteInfo { get; set; } = (1, "Cliente", "cliente@teste.com");

        public Task<bool> ClienteExisteEstaAtivoAsync(int clienteId)
            => Task.FromResult(ClienteExiste);

        public Task<(int Id, string Nome, string Email)?> ObterInfoClienteAsync(int clienteId)
            => Task.FromResult(ClienteInfo);
    }

    private sealed class FakeVeiculoService : IVeiculoService
    {
        public bool VeiculoPertence { get; set; } = true;

        public Task<(int Id, string Modelo, string Placa)?> ObterInfoVeiculoAsync(int veiculoId)
            => Task.FromResult<(int, string, string)?>(null);

        public Task<bool> VeiculoPertenceAoClienteAsync(int veiculoId, int clienteId)
            => Task.FromResult(VeiculoPertence);
    }

    private sealed class FakeNotificacaoService : INotificacaoService
    {
        public int EnviarConfirmacaoCalls { get; private set; }

        public Task<bool> EnviarConfirmacaoEntregaAsync(string clienteEmail, int numeroOS)
        {
            EnviarConfirmacaoCalls++;
            return Task.FromResult(true);
        }
    }
}
