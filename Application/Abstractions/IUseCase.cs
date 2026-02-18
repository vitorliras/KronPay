using Shared.Results;

namespace Application.Abstractions;

public interface IUseCase<in TRequest, TResponse>
{
    Task<ResultEntity<TResponse>> ExecuteAsync(TRequest request);
}
public interface IUseCaseWithoutRequest<TResponse>
{
    Task<ResultEntity<TResponse>> ExecuteAsync();
}