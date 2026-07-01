using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.UseCases.Planning;
using Domain.Entities.Planning;
using Domain.Interfaces;
using Domain.Interfaces.Planning;
using Moq;
using Shared.Localization;
using Shouldly;

namespace Tests.Application.Planning;

public class PlannedCommitmentUseCasesTests
{
    private readonly Mock<IPlannedCommitmentRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    public PlannedCommitmentUseCasesTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Create_persiste_e_retorna_o_compromisso()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<PlannedCommitment>())).ReturnsAsync(true);

        var sut = new CreatePlannedCommitmentUseCase(_repo.Object, _uow.Object, _currentUser.Object);

        var request = new CreatePlannedCommitmentRequest(
            "Aluguel", 1500m, "O", "M", new DateTime(2026, 1, 5), null, null);

        var result = await sut.ExecuteAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Description.ShouldBe("Aluguel");
        result.Value!.Active.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_de_compromisso_inexistente_falha()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((PlannedCommitment?)null);

        var sut = new UpdatePlannedCommitmentUseCase(_repo.Object, _uow.Object, _currentUser.Object);

        var request = new UpdatePlannedCommitmentRequest(
            99, "Aluguel", 1500m, "O", "M", new DateTime(2026, 1, 5), null, null);

        var result = await sut.ExecuteAsync(request);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.PlannedCommitmentNotFound);
    }

    [Fact]
    public async Task Deactivate_desativa_o_compromisso()
    {
        var commitment = new PlannedCommitment(1, "Aluguel", 1500m, "O", "M", new DateTime(2026, 1, 5));
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(commitment);
        _repo.Setup(r => r.Update(It.IsAny<PlannedCommitment>())).Returns(true);

        var sut = new DeactivatePlannedCommitmentUseCase(_repo.Object, _uow.Object, _currentUser.Object);

        var result = await sut.ExecuteAsync(new DeactivatePlannedCommitmentRequest(1));

        result.IsSuccess.ShouldBeTrue();
        commitment.Active.ShouldBeFalse();
    }
}
