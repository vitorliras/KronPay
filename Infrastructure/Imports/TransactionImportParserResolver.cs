using Application.Abstractions.Import;
using Domain.Exceptions;
using Shared.Localization;

public sealed class TransactionImportParserResolver
{
    private readonly IEnumerable<ITransactionImportParser> _parsers;

    public TransactionImportParserResolver(IEnumerable<ITransactionImportParser> parsers)
    {
        _parsers = parsers;
    }

    public ITransactionImportParser Resolve(string fileName)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));

        if (parser is null)
            throw new DomainException(MessageKeys.InvalidDescription);


        return parser;
    }
}
