namespace Application.Abstractions.AI;

public interface IAiDocumentReader
{
    Task<AiDocument> ReadAsync(Stream stream);
}
