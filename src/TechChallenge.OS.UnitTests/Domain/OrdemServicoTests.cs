using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Domain.Common.Enums;
using TechChallenge.OS.Domain.Exceptions;

namespace TechChallenge.OS.UnitTests.Domain;

public class OrdemServicoTests
{
    [Fact]
    public void Criar_DeveCriarOS_QuandoDadosValidos()
    {
        var os = OrdemServico.Criar(1, 2, 100);

        Assert.Equal(1, os.ClienteId);
        Assert.Equal(2, os.VeiculoId);
        Assert.Equal(100, os.Numero);
        Assert.Equal(StatusOrdemServico.Recebida, os.Status);
        Assert.NotEqual(Guid.Empty, os.UniqueCode);
        Assert.Empty(os.ItensServicos);
        Assert.Empty(os.ItensPecasInsumos);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    public void Criar_DeveFalhar_QuandoDadosInvalidos(int clienteId, int veiculoId, int numero)
    {
        Assert.Throws<DomainException>(() => OrdemServico.Criar(clienteId, veiculoId, numero));
    }

    [Fact]
    public void AdicionarServico_DevePreservarSnapshotInicial()
    {
        var os = OrdemServico.Criar(1, 2, 10);

        os.AdicionarServico(11, "Troca de óleo");

        var item = Assert.Single(os.ItensServicos);
        Assert.Equal(11, item.ServicoId);
        Assert.Equal("Troca de óleo", item.Descricao);
        Assert.NotEqual(Guid.Empty, item.UniqueCode);
    }

    [Fact]
    public void AdicionarPecaInsumo_DevePreservarSnapshotInicial()
    {
        var os = OrdemServico.Criar(1, 2, 10);

        os.AdicionarPecaInsumo(22, "Filtro", 2);

        var item = Assert.Single(os.ItensPecasInsumos);
        Assert.Equal(22, item.PecaInsumoId);
        Assert.Equal("Filtro", item.Descricao);
        Assert.Equal(2, item.Quantidade);
        Assert.NotEqual(Guid.Empty, item.UniqueCode);
    }

    [Fact]
    public void AdicionarItens_DeveFalhar_QuandoStatusNaoRecebida()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarDiagnosticoIniciado();

        Assert.Throws<InvalidOperationException>(() => os.AdicionarServico(11, "Tardio"));
        Assert.Throws<InvalidOperationException>(() => os.AdicionarPecaInsumo(22, "Tardia", 1));
    }

    [Fact]
    public void ItemServico_Criar_DeveFalhar_QuandoInvalido()
    {
        Assert.Throws<DomainException>(() => OrdemServicoItemServico.Criar(0, "Troca"));
        Assert.Throws<DomainException>(() => OrdemServicoItemServico.Criar(1, ""));
        Assert.Throws<DomainException>(() => OrdemServicoItemServico.Criar(1, "   "));
    }

    [Fact]
    public void ItemPecaInsumo_Criar_DeveFalhar_QuandoInvalido()
    {
        Assert.Throws<DomainException>(() => OrdemServicoItemPecaInsumo.Criar(0, "Filtro", 1));
        Assert.Throws<DomainException>(() => OrdemServicoItemPecaInsumo.Criar(1, "", 1));
        Assert.Throws<DomainException>(() => OrdemServicoItemPecaInsumo.Criar(1, "Filtro", 0));
    }

    [Fact]
    public void FluxoValido_ComAprovacao_DeveChegarEmEntregue()
    {
        var os = OrdemServico.Criar(1, 2, 10);

        os.RegistrarDiagnosticoIniciado();
        os.RegistrarAguardandoAprovacaoOrcamento();
        os.RegistrarOrcamentoAprovado();
        os.RegistrarExecucaoIniciada();
        os.RegistrarExecucaoFinalizada();
        os.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
    }

    [Fact]
    public void FluxoValido_ComReprovacaoDepoisAprovacao_DeveChegarEmEntregue()
    {
        var os = OrdemServico.Criar(1, 2, 10);

        os.RegistrarDiagnosticoIniciado();
        os.RegistrarAguardandoAprovacaoOrcamento();
        os.RegistrarOrcamentoReprovado();
        os.RegistrarOrcamentoAprovado();
        os.RegistrarExecucaoIniciada();
        os.RegistrarExecucaoFinalizada();
        os.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
    }

    [Fact]
    public void RegistrarAguardandoAprovacao_DevePermitirPartindoRecebidaOuDiagnostico()
    {
        var osRecebida = OrdemServico.Criar(1, 2, 10);
        osRecebida.RegistrarAguardandoAprovacaoOrcamento();
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, osRecebida.Status);

        var osDiagnostico = OrdemServico.Criar(1, 2, 11);
        osDiagnostico.RegistrarDiagnosticoIniciado();
        osDiagnostico.RegistrarAguardandoAprovacaoOrcamento();
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, osDiagnostico.Status);
    }

    [Fact]
    public void RegistrarOrcamentoAprovadoOuReprovado_DeveManterStatusAguardandoAprovacao()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarAguardandoAprovacaoOrcamento();

        os.RegistrarOrcamentoAprovado();
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);

        os.RegistrarOrcamentoReprovado();
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
    }

    [Fact]
    public void RegistrarDiagnosticoIniciado_DeveFalhar_QuandoNaoRecebida()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarDiagnosticoIniciado();

        Assert.Throws<InvalidOperationException>(() => os.RegistrarDiagnosticoIniciado());
    }

    [Fact]
    public void RegistrarAguardandoAprovacao_DeveFalhar_QuandoStatusInvalido()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarAguardandoAprovacaoOrcamento();
        os.RegistrarExecucaoIniciada();

        Assert.Throws<InvalidOperationException>(() => os.RegistrarAguardandoAprovacaoOrcamento());
    }

    [Fact]
    public void RegistrarOrcamentoAprovado_DeveFalhar_QuandoNaoAguardandoAprovacao()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        Assert.Throws<InvalidOperationException>(() => os.RegistrarOrcamentoAprovado());
    }

    [Fact]
    public void RegistrarOrcamentoReprovado_DeveFalhar_QuandoNaoAguardandoAprovacao()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        Assert.Throws<InvalidOperationException>(() => os.RegistrarOrcamentoReprovado());
    }

    [Fact]
    public void RegistrarExecucaoIniciada_DeveFalhar_QuandoNaoAguardandoAprovacao()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        Assert.Throws<InvalidOperationException>(() => os.RegistrarExecucaoIniciada());
    }

    [Fact]
    public void RegistrarExecucaoFinalizada_DeveFalhar_QuandoNaoEmExecucao()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        Assert.Throws<InvalidOperationException>(() => os.RegistrarExecucaoFinalizada());
    }

    [Fact]
    public void Entregar_DeveFalhar_QuandoNaoFinalizada()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        Assert.Throws<InvalidOperationException>(() => os.Entregar());
    }

    [Fact]
    public void Delete_DeveRealizarExclusaoLogica_SemAlterarStatus()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.RegistrarDiagnosticoIniciado();
        var statusAntes = os.Status;

        os.Delete();

        Assert.False(os.Ativo);
        Assert.Equal(statusAntes, os.Status);
    }

    [Fact]
    public void Delete_DeveSerIdempotente()
    {
        var os = OrdemServico.Criar(1, 2, 10);

        os.Delete();
        os.Delete();

        Assert.False(os.Ativo);
    }
}
