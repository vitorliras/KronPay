public sealed record ImportTransactionsRequest(
    Stream FileStream,
    string FileName,
    bool Preview,
    bool UseAi
);