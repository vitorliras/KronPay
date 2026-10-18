using Application.Abstractions.AiChat;
using Application.Configuration.AI;
using Infrastructure.AI.Chat;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Tests.Infrastructure.AI;

public class InMemoryChatAttachmentStoreTests
{
    private readonly AiSettings _settings = new() { Chat = new AiChatSettings { MaxAttachmentsPerConversation = 3, AttachmentTtlMinutes = 60 } };
    private readonly InMemoryChatAttachmentStore _store;

    public InMemoryChatAttachmentStoreTests()
    {
        var monitor = new Mock<IOptionsMonitor<AiSettings>>();
        monitor.Setup(m => m.CurrentValue).Returns(_settings);
        _store = new InMemoryChatAttachmentStore(monitor.Object);
    }

    private static ChatAttachment Attachment(string id, DateTime? createdUtc = null) =>
        new(id, $"{id}.pdf", Array.Empty<AttachmentTransaction>(), createdUtc ?? DateTime.UtcNow);

    [Fact]
    public void Guarda_e_devolve_na_ordem_pedida_ignorando_os_inexistentes()
    {
        _store.Add(1, Attachment("a"));
        _store.Add(1, Attachment("b"));

        _store.GetMany(1, new[] { "b", "x", "a" }).Select(a => a.Id).ShouldBe(new[] { "b", "a" });
    }

    [Fact]
    public void Anexo_de_um_usuario_nao_aparece_para_outro()
    {
        _store.Add(1, Attachment("a"));

        _store.Get(2, "a").ShouldBeNull();
        _store.Get(1, "a").ShouldNotBeNull();
    }

    [Fact]
    public void Acima_do_limite_remove_os_mais_antigos()
    {
        foreach (var id in new[] { "a", "b", "c", "d" })
            _store.Add(1, Attachment(id));

        _store.Get(1, "a").ShouldBeNull();
        _store.GetMany(1, new[] { "b", "c", "d" }).Count.ShouldBe(3);
    }

    [Fact]
    public void Expirados_sao_descartados()
    {
        _store.Add(1, Attachment("old", DateTime.UtcNow.AddHours(-2)));
        _store.Add(1, Attachment("new"));

        _store.Get(1, "old").ShouldBeNull();
        _store.Get(1, "new").ShouldNotBeNull();
    }
}
