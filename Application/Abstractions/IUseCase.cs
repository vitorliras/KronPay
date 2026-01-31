using Shared.Results;

namespace Application.Abstractions;

public interface IUseCase<in TRequest, TResponse>
{
    Task<ResultT<TResponse>> ExecuteAsync(TRequest request);
}
