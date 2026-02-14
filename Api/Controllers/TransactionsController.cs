using Api.DTOs.Transactions;
using Api.Extensions;
using Application.DTOs.Transactions;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;

[Authorize]
[ApiController]
[Route("transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateTransactionUseCase _create;
    private readonly CreateTransactionRangeUseCase _createRange;
    private readonly UpdateTransactionUseCase _update;
    private readonly ChangeStatusTransactionUseCase _changeStatus;
    private readonly DeleteTransactionUseCase _delete;
    private readonly DeleteTransactionRangeUseCase _deleteRange;
    private readonly GetTransactionsByIdGroupUseCase _getAllByGroup;
    private readonly GetTransactionsByMonthUseCase _getAllByMonth;
    private readonly GetTransactionsByYearUseCase _getAllByYear;
    private readonly ImportTransactionsUseCase _import;
    private readonly IStringLocalizer<Messages> _localizer;

    public TransactionsController(
        UseCaseExecutor executor,
        CreateTransactionUseCase create,
        CreateTransactionRangeUseCase createRange,
        UpdateTransactionUseCase update,
        ChangeStatusTransactionUseCase changeStatus,
        DeleteTransactionUseCase delete,
        DeleteTransactionRangeUseCase deleteRange,
        GetTransactionsByIdGroupUseCase getAllByGroup,
        GetTransactionsByMonthUseCase getAllByMonth,
        GetTransactionsByYearUseCase getAllByYear,
        ImportTransactionsUseCase import,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _createRange = createRange;
        _update = update;
        _changeStatus = changeStatus;
        _delete = delete;
        _deleteRange = deleteRange;
        _getAllByGroup = getAllByGroup;
        _getAllByMonth = getAllByMonth;
        _getAllByYear = getAllByYear;
        _getAllByYear = getAllByYear;
        _localizer = localizer;
        _import = import;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllByGroup(
        [FromQuery] GetTransactionsByGroupRequest request,
        [FromServices] ValidationPipeline<GetTransactionsByGroupRequest, IEnumerable<TransactionFullResponse>> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _getAllByGroup,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllByMonth(
       [FromQuery] GetTransactionsByMonthRequest request,
       [FromServices] ValidationPipeline<GetTransactionsByMonthRequest, IEnumerable<TransactionFullResponse>> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _getAllByMonth,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }


    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllByYear(
       [FromQuery] GetTransactionsByYearRequest request,
       [FromServices] ValidationPipeline<GetTransactionsByYearRequest, IEnumerable<TransactionFullResponse>> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _getAllByYear,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTransactionRequest request,
        [FromServices] ValidationPipeline<CreateTransactionRequest, TransactionResponse> pipeline)
    {
        
        var result = await _executor.ExecuteAsync(
            request,
            _create,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateRange(
       TransactionRangeRequest request,
       [FromServices] ValidationPipeline<TransactionRangeRequest, TransactionRangeResponse> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _createRange,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> Update(
        UpdateTransactionRequest request,
        [FromServices] ValidationPipeline<UpdateTransactionRequest, TransactionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _update,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> ChangeStatus(
      ChangeStatusTransactionRequest request,
      [FromServices] ValidationPipeline<ChangeStatusTransactionRequest, TransactionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _changeStatus,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> Delete(
    DeleteTransactionRequest request,
    [FromServices] ValidationPipeline<DeleteTransactionRequest, TransactionResponse> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _delete,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteRange(
       TransactionRangeRequest request,
       [FromServices] ValidationPipeline<TransactionRangeRequest, TransactionRangeResponse> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _deleteRange,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(
        [FromForm] ImportTransactionsFormRequest form,
        [FromServices] ValidationPipeline<ImportTransactionsRequest, ImportTransactionsResponse> pipeline)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("File is required");

        await using var stream = form.File.OpenReadStream();

        var request = new ImportTransactionsRequest(
            form.UserId,
            stream,
            form.File.FileName,
            form.Preview,
            form.UseAi
        );

        var result = await _executor.ExecuteAsync(
            request,
            _import,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }


}
