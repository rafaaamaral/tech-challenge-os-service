using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Interfaces.Services;
using TechChallenge.OS.Infrastructure.Messaging;
using TechChallenge.OS.Infrastructure.Persistence;

namespace TechChallenge.OS.UnitTests.Messaging;

public class MessagingTests
{
    private static IntegrationEventHandler CreateHandler()
    {
        var options = new DbContextOptionsBuilder<OSDbContext>()
            .UseNpgsql("Host=invalid;Database=x;Username=x;Password=x").Options;
        return new IntegrationEventHandler(new OSDbContext(options), NullLogger<IntegrationEventHandler>.Instance);
    }

    [Fact]
    public async Task Handler_JsonInvalido_DeveSerPermanente()
    {
        await Assert.ThrowsAsync<PermanentMessageException>(() =>
            CreateHandler().HandleAsync("billing.aprovado", Encoding.UTF8.GetBytes("{nope"), default));
    }

    [Fact]
    public async Task Handler_EventoSemCamposObrigatorios_DeveSerPermanente()
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new { eventId = Guid.Empty, ordemServicoUniqueCode = Guid.NewGuid(), occurredAt = DateTime.UtcNow });
        await Assert.ThrowsAsync<PermanentMessageException>(() =>
            CreateHandler().HandleAsync("execution.finalizada", body, default));
    }

    [Fact]
    public async Task Handler_RoutingKeyDesconhecida_DeveSerPermanente()
    {
        await Assert.ThrowsAsync<PermanentMessageException>(() =>
            CreateHandler().HandleAsync("foo.bar", Encoding.UTF8.GetBytes("{}"), default));
    }

    [Fact]
    public async Task Decorator_DevePublicarAposCriar_ENaoFalharSePublisherFalhar()
    {
        var inner = new Mock<IOrdemServicoApplicationService>();
        var dto = new OrdemServicoResponseDto { UniqueCode = Guid.NewGuid() };
        inner.Setup(x => x.CriarAsync(It.IsAny<CreateOrdemServicoDto>())).ReturnsAsync(dto);
        var publisher = new Mock<IOrdemServicoEventPublisher>();
        publisher.Setup(x => x.PublicarOrdemServicoCriadaAsync(dto.UniqueCode, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("broker down"));

        var sut = new PublishingOrdemServicoApplicationService(inner.Object, publisher.Object,
            NullLogger<PublishingOrdemServicoApplicationService>.Instance);

        var result = await sut.CriarAsync(new CreateOrdemServicoDto { ClienteId = 1, VeiculoId = 1 });

        Assert.Same(dto, result);
        publisher.Verify(x => x.PublicarOrdemServicoCriadaAsync(dto.UniqueCode, It.IsAny<CancellationToken>()), Times.Once);
    }
}
