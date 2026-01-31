using Application.DTOs.Users;
using Application.Pipelines;
using Application.UseCases.Users;
using Application.Validators.Users;
using Domain.interfaces;
using Moq;
using Shouldly;

namespace Tests.Application.Pipelines;

public class ValidationPipelineTests
{
    //[Fact]
    //public async Task Should_fail_when_request_is_invalid()
    //{
    //    var validators = new[] { new CreateUserValidator() };
    //    var pipeline = new ValidationPipeline<CreateUserRequest, object>(validators);

    //    var result = await pipeline.ValidateAsync(
    //        new CreateUserRequest("", ""),
    //        () => Task.FromResult(Shared.Results.ResultT<object>.Success(null!))
    //    );

    //    result.IsFailure.ShouldBeTrue();
    //    result.Error!.Code.ShouldBe("VALIDATION_ERROR");
    //}
}
