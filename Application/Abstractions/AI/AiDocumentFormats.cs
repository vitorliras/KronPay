namespace Application.Abstractions.AI;

public static class AiDocumentFormats
{
    private static readonly string[] AiOnlyExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".xlsx", ".xls" };

    public static bool IsAiOnly(string fileName) =>
        AiOnlyExtensions.Any(extension => fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
}
