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
    }

    [Fact]
    public async Task Should_create_user_when_not_exists()
    {
    }

    [Fact]
    public async Task Should_return_failure_when_user_exists()
    {
    }
}
