public sealed record ImportTransactionsRequest(
    int UserId,
    Stream FileStream,
    string FileName,
    bool Preview,
    bool UseAi
);