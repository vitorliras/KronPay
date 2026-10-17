using Application.Configuration.AI;
using Application.Services.AI;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Tests.Application.AI;

public class AiAvailabilityTests
{
    private static AiAvailability Build(AiSettings settings)
    {
        var monitor = new Mock<IOptionsMonitor<AiSettings>>();
        monitor.Setup(m => m.CurrentValue).Returns(settings);
        return new AiAvailability(monitor.Object);
    }

    private static AiProviderSettings Provider(string apiKey = "sk-test") => new()
    {
        BaseUrl = "https://api.example.com/v1",
        Model = "model-x",
        ApiKey = apiKey
    };

    [Fact]
    public void Desligada_nao_retorna_provedor_mesmo_com_tudo_configurado()
    {
        var settings = new AiSettings
        {
            Enabled = false,
            Provider = "OpenAI",
            Providers = { ["OpenAI"] = Provider() }
        };

        Build(settings).GetActive().ShouldBeNull();
    }

    [Fact]
    public void Ligada_com_provedor_completo_retorna_o_provedor_ativo()
    {
        var settings = new AiSettings
        {
            Enabled = true,
            Provider = "openai",
            Providers = { ["OpenAI"] = Provider() }
        };

        var active = Build(settings).GetActive();

        active.ShouldNotBeNull();
        active.Name.ShouldBe("OpenAI");
        active.Settings.Model.ShouldBe("model-x");
    }

    [Fact]
    public void Ligada_sem_chave_nao_retorna_provedor()
    {
        var settings = new AiSettings
        {
            Enabled = true,
            Provider = "OpenAI",
            Providers = { ["OpenAI"] = Provider(apiKey: "") }
        };

        Build(settings).GetActive().ShouldBeNull();
    }

    [Fact]
    public void Ollama_nao_exige_chave()
    {
        var settings = new AiSettings
        {
            Enabled = true,
            Provider = "Ollama",
            Providers = { ["Ollama"] = Provider(apiKey: "") }
        };

        Build(settings).GetActive().ShouldNotBeNull();
    }

    [Fact]
    public void Provedor_inexistente_nao_retorna_provedor()
    {
        var settings = new AiSettings
        {
            Enabled = true,
            Provider = "Inexistente",
            Providers = { ["OpenAI"] = Provider() }
        };

        Build(settings).GetActive().ShouldBeNull();
    }

    [Fact]
    public void Provedor_sem_modelo_nao_retorna_provedor()
    {
        var provider = Provider();
        provider.Model = "";

        var settings = new AiSettings
        {
            Enabled = true,
            Provider = "OpenAI",
            Providers = { ["OpenAI"] = provider }
        };

        Build(settings).GetActive().ShouldBeNull();
    }
}
