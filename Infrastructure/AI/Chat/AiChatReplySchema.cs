using System.Text.Json;

namespace Infrastructure.AI.Chat;

internal static class AiChatReplySchema
{
    public const string Name = "chat_reply";

    public static readonly JsonElement Schema = JsonDocument.Parse(@"{
  ""type"": ""object"",
  ""properties"": {
    ""reply"": { ""type"": ""string"" },
    ""suggestions"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } }
  },
  ""required"": [""reply"", ""suggestions""],
  ""additionalProperties"": false
}").RootElement.Clone();
}
