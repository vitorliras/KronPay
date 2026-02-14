using FluentValidation;
using Shared.Results;

namespace Application.Pipelines;

public sealed class ValidationPipeline<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipeline(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<ResultEntity<TResponse>> ValidateAsync(
        TRequest request,
        Func<Task<ResultEntity<TResponse>>> next)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Any())
        {
            return ResultEntity<TResponse>.Failure(failures.First().ErrorMessage);
        }

        return await next();
    }
}
