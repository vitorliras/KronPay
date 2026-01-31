using Application.DTOs.Users;
using Application.UseCases.Users;
using Domain.Entities;
using Domain.interfaces;
using Moq;
using Shouldly;

namespace Tests.Application.UseCases;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _useCase = new CreateUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task Should_create_user_when_not_exists()
    {
        _repositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserValueObject?)null);

        var request = new CreateUserRequest("Vitor", "vitor@email.com");

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Email.ShouldBe("vitor@email.com");
    }

    [Fact]
    public async Task Should_return_failure_when_user_exists()
    {
        _repositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserValueObject("Vitor", new("vitor@email.com")));

        var request = new CreateUserRequest("Vitor", "vitor@email.com");

        var result = await _useCase.ExecuteAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error!.Code.ShouldBe("USER_ALREADY_EXISTS");
    }
}
