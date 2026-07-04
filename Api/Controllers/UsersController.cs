using Api.Extensions;
using Application.DTOs.Users;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;

[Authorize]
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateUserUseCase _createUseCase;
    private readonly GetUserUseCase _getUseCase;
    private readonly UploadProfilePhotoUseCase _uploadPhotoUseCase;
    private readonly RemoveProfilePhotoUseCase _removePhotoUseCase;
    private readonly GetProfilePhotoUseCase _getPhotoUseCase;
    private readonly IStringLocalizer<Messages> _localizer;


    public UsersController(
        UseCaseExecutor executor,
        CreateUserUseCase createUseCase,
        GetUserUseCase getUseCase,
        UploadProfilePhotoUseCase uploadPhotoUseCase,
        RemoveProfilePhotoUseCase removePhotoUseCase,
        GetProfilePhotoUseCase getPhotoUseCase,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _uploadPhotoUseCase = uploadPhotoUseCase;
        _removePhotoUseCase = removePhotoUseCase;
        _getPhotoUseCase = getPhotoUseCase;
        _localizer = localizer;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
    CreateUserRequest request,
    [FromServices] ValidationPipeline<CreateUserRequest, UserResponse> pipeline,
    [FromServices] IStringLocalizer<Messages> localizer)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _createUseCase,
            pipeline
        );

        return result.ToActionResult(localizer);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var result = await _executor.ExecuteAsync(_getUseCase);

        return result.ToActionResult(_localizer);
    }

    [Consumes("multipart/form-data")]
    [HttpPost("[action]")]
    public async Task<IActionResult> UploadPhoto(
        [FromForm] IFormFile? file,
        [FromServices] ValidationPipeline<UploadProfilePhotoRequest, ProfilePhotoResponse> pipeline)
    {
        if (file is null || file.Length == 0)
            return ResultEntity<ProfilePhotoResponse>.Failure(MessageKeys.InvalidImageFormat).ToActionResult(_localizer);

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var request = new UploadProfilePhotoRequest(stream.ToArray(), file.ContentType);

        var result = await _executor.ExecuteAsync(request, _uploadPhotoUseCase, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> RemovePhoto()
    {
        var result = await _executor.ExecuteAsync(_removePhotoUseCase);
        return result.ToActionResult(_localizer);
    }

    // Exceção consciente ao contrato ResultEntity<T> (integration.md A.1): uma tag <img>
    // não consegue consumir um envelope JSON, então este endpoint devolve os bytes crus.
    [HttpGet("[action]")]
    public async Task<IActionResult> Photo()
    {
        var result = await _executor.ExecuteAsync(_getPhotoUseCase);

        if (!result.IsSuccess || result.Value is null)
            return NotFound();

        return File(result.Value.Content, result.Value.ContentType);
    }
}
