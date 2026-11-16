using TechChallenge.OS.Application.Mappers;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Domain.Common.Enums;

namespace TechChallenge.OS.UnitTests.Application;

public class OrdemServicoMapperTests
{
    [Fact]
    public void ToResponseDto_DeveMapearCamposBasicosESnapshots()
    {
        var os = OrdemServico.Criar(1, 2, 10);
        os.AdicionarServico(11, "Troca de óleo");
        os.AdicionarPecaInsumo(22, "Filtro", 2);
        os.RegistrarAguardandoAprovacaoOrcamento();

        var dto = os.ToResponseDto();

        Assert.Equal(os.Id, dto.Id);
        Assert.Equal(os.UniqueCode, dto.UniqueCode);
        Assert.Equal(1, dto.ClienteId);
        Assert.Equal(2, dto.VeiculoId);
        Assert.Equal(10, dto.Numero);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, dto.Status);
        Assert.Single(dto.ItensServicos);
        Assert.Single(dto.ItensPecasInsumos);
    }

    [Fact]
    public void ToResponseDtoList_DeveMapearColecao()
    {
        var os1 = OrdemServico.Criar(1, 2, 10);
        var os2 = OrdemServico.Criar(1, 3, 11);

        var list = new[] { os1, os2 };

        var dtos = list.ToResponseDtoList();

        Assert.Equal(2, dtos.Count);
    }

    [Fact]
    public void ToResponseDto_DeveLancarQuandoNulo()
    {
        OrdemServico? os = null;

        Assert.Throws<ArgumentNullException>(() => os!.ToResponseDto());
    }

    [Fact]
    public void ToResponseDtoList_DeveLancarQuandoNulo()
    {
        IEnumerable<OrdemServico>? list = null;

        Assert.Throws<ArgumentNullException>(() => list!.ToResponseDtoList());
    }
}
