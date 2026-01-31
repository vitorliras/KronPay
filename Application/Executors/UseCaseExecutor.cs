using Application.Abstractions;
using Application.Pipelines;
using Shared.Results;

namespace Application.Executors;

public sealed class UseCaseExecutor
{
    public async Task<ResultT<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        IUseCase<TRequest, TResponse> useCase,
        ValidationPipeline<TRequest, TResponse> pipeline)
    {
        return await pipeline.ValidateAsync(
            request,
            () => useCase.ExecuteAsync(request)
        );
    }
}
