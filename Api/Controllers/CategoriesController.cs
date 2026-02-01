using Api.Extensions;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Configuration.Category;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;

[Authorize]
[ApiController]
[Route("categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateCategoryUseCase _create;
    private readonly UpdateCategoryUseCase _update;
    private readonly DeactivateCategoryUseCase _deactivate;
    private readonly GetAllCategoriesUseCase _getAll;
    private readonly GetCategoryByIdUseCase _getById;
    private readonly IStringLocalizer<Messages> _localizer;

    public CategoriesController(
        UseCaseExecutor executor,
        CreateCategoryUseCase create,
        UpdateCategoryUseCase update,
        DeactivateCategoryUseCase deactivate,
        GetAllCategoriesUseCase getAll,
        GetCategoryByIdUseCase getById,
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
        [FromServices] ValidationPipeline<GetAllCategoriesRequest, IEnumerable<CategoryResponse>> pipeline)
    {
        var request = new GetAllCategoriesRequest(userId);

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
        [FromServices] ValidationPipeline<GetCategoryByIdRequest, CategoryResponse> pipeline)
    {
        var request = new GetCategoryByIdRequest(id, userId);

        var result = await _executor.ExecuteAsync(
            request,
            _getById,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryRequest request,
        [FromServices] ValidationPipeline<CreateCategoryRequest, CategoryResponse> pipeline)
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
        UpdateCategoryRequest request,
        [FromServices] ValidationPipeline<UpdateCategoryRequest, CategoryResponse> pipeline)
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
    DeactivateCategoryRequest request,
    [FromServices] ValidationPipeline<DeactivateCategoryRequest, Unit> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _deactivate,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

}
