using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Banks;
using Doamain.Interface.Banks;
using Domain.Entities.banks;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Banks;

public sealed class CreateBankConnectionUseCase
    : IUseCase<CreateBankConnectionRequest, BankConnectionResponse>
{
    private readonly IBankRepository _bankRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateBankConnectionUseCase(
        IBankRepository bankRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _bankRepository = bankRepository;
        _uow = uow;
        _currentUser = currentUser;
    }
    public async Task<ResultEntity<BankConnectionResponse>> ExecuteAsync(
        CreateBankConnectionRequest request)
    {
        var userId = _currentUser.UserId;

        var existing = await _bankRepository
            .GetByExternalConnectionIdAsync(request.ExternalConnectionId);

        if (existing is not null)
            return ResultEntity<BankConnectionResponse>.Failure(
                MessageKeys.DescriptionAlreadyExists);

        var connection = new BankConnection
        {
            UserId = userId,
            ExternalConnectionId = request.ExternalConnectionId,
            InstitutionCode = request.InstitutionCode,
            InstitutionName = request.InstitutionName,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        await _bankRepository.AddAsync(connection);
        var uow = await _uow.CommitAsync();

        if (!uow)
            return ResultEntity<BankConnectionResponse>.Failure(
                MessageKeys.DataPersistenceFailed);

        return ResultEntity<BankConnectionResponse>.Success(
            new BankConnectionResponse(
                connection.Id,
                connection.InstitutionName,
                connection.Active
            ),
            MessageKeys.OperationSuccess
        );
    }
}