using Application.Configuration.AI;

namespace Application.Abstractions.AI;

public sealed record ActiveAiProvider(string Name, AiProviderSettings Settings);
