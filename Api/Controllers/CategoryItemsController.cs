using Api.Extensions;
using Application.Executors;
using Application.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;
using Application.UseCases.Categories;
using Application.DTOs.Configuration.CategoryItems;

[Authorize]
[ApiController]
[Route("categoryitems")]
public sealed class CategoryItemsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateCategoryItemUseCase _create;
    private readonly UpdateCategoryItemUseCase _update;
    private readonly DeactivateCategoryItemUseCase _deactivate;
    private readonly GetAllCategoryItemsUseCase _getAll;
    private readonly GetCategoryItemByIdUseCase _getById;
    private readonly IStringLocalizer<Messages> _localizer;

    public CategoryItemsController(
        UseCaseExecutor executor,
        CreateCategoryItemUseCase create,
        UpdateCategoryItemUseCase update,
        DeactivateCategoryItemUseCase deactivate,
        GetAllCategoryItemsUseCase getAll,
        GetCategoryItemByIdUseCase getById,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _update = update;
        _deactivate = deactivate;
        _getAll = getAll;
        _getById = getById;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int userId,
        [FromServices] ValidationPipeline<GetAllCategoryItemsRequest, IEnumerable<CategoryItemResponse>> pipeline)
    {
        var request = new GetAllCategoryItemsRequest(userId);

        var result = await _executor.ExecuteAsync(
            request,
            _getAll,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] int userId,
        [FromServices] ValidationPipeline<GetCategoryItemByIdRequest, CategoryItemResponse> pipeline)
    {
        var request = new GetCategoryItemByIdRequest(id, userId);

        var result = await _executor.ExecuteAsync(
            request,
            _getById,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryItemRequest request,
        [FromServices] ValidationPipeline<CreateCategoryItemRequest, CategoryItemResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _create,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateCategoryItemRequest request,
        [FromServices] ValidationPipeline<UpdateCategoryItemRequest, CategoryItemResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _update,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpDelete]
    public async Task<IActionResult> Deactivate(
    DeactivateCategoryItemRequest request,
    [FromServices] ValidationPipeline<DeactivateCategoryItemRequest, Unit> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _deactivate,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

}
