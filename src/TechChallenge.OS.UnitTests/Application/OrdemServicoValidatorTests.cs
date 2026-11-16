using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Validators;

namespace TechChallenge.OS.UnitTests.Application;

public class OrdemServicoValidatorTests
{
    [Fact]
    public void ValidarCreateOrdemServicoDto_DevePassar_QuandoValido()
    {
        var dto = new CreateOrdemServicoDto
        {
            ClienteId = 1,
            VeiculoId = 2,
            ItensServicos = [new CreateOrdemServicoItemServicoDto { ServicoId = 10, Descricao = "Troca" }],
            ItensPecasInsumos = [new CreateOrdemServicoItemPecaInsumoDto { PecaInsumoId = 20, Descricao = "Filtro", Quantidade = 1 }]
        };

        OrdemServicoValidator.ValidarCreateOrdemServicoDto(dto);
    }

    [Fact]
    public void ValidarCreateOrdemServicoDto_DeveFalhar_QuandoClienteInvalido()
    {
        var dto = new CreateOrdemServicoDto
        {
            ClienteId = 0,
            VeiculoId = 2,
            ItensServicos = [new CreateOrdemServicoItemServicoDto { ServicoId = 10 }]
        };

        Assert.Throws<InvalidOperationException>(() => OrdemServicoValidator.ValidarCreateOrdemServicoDto(dto));
    }

    [Fact]
    public void ValidarCreateOrdemServicoDto_DeveFalhar_QuandoVeiculoInvalido()
    {
        var dto = new CreateOrdemServicoDto
        {
            ClienteId = 1,
            VeiculoId = 0,
            ItensServicos = [new CreateOrdemServicoItemServicoDto { ServicoId = 10 }]
        };

        Assert.Throws<InvalidOperationException>(() => OrdemServicoValidator.ValidarCreateOrdemServicoDto(dto));
    }

    [Fact]
    public void ValidarCreateOrdemServicoDto_DeveFalhar_QuandoSemItens()
    {
        var dto = new CreateOrdemServicoDto { ClienteId = 1, VeiculoId = 2 };

        Assert.Throws<InvalidOperationException>(() => OrdemServicoValidator.ValidarCreateOrdemServicoDto(dto));
    }

    [Fact]
    public void ValidarCreateOrdemServicoItemServicoDto_DeveFalhar_QuandoInvalido()
    {
        Assert.Throws<InvalidOperationException>(() => OrdemServicoValidator.ValidarCreateOrdemServicoItemServicoDto(new CreateOrdemServicoItemServicoDto { ServicoId = 0 }));
    }

    [Fact]
    public void ValidarCreateOrdemServicoItemPecaInsumoDto_DeveFalhar_QuandoInvalido()
    {
        Assert.Throws<InvalidOperationException>(() => OrdemServicoValidator.ValidarCreateOrdemServicoItemPecaInsumoDto(new CreateOrdemServicoItemPecaInsumoDto { PecaInsumoId = 0, Quantidade = 0 }));
    }
}
