using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.PaymentMethods;
using Application.DTOs.Users;
using Domain.interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class GetUserUseCase
    : IUseCaseWithoutRequest<UserAllDatasResponse>
{
    private readonly IUserRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetUserUseCase(IUserRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<UserAllDatasResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var user = await _repository.GetByIdAsync(userId);

        if (user == null)
            return ResultEntity<UserAllDatasResponse>.Failure(MessageKeys.InvaldUser);


        return ResultEntity<UserAllDatasResponse>.Success(
               new UserAllDatasResponse(
                   user.Name.Value,
                   user.Username.Value,
                   user.Phone.Value,
                   user.Cpf.Value,
                   user.UserType,
                   user.Email.Value
               ), MessageKeys.OperationSuccess
           );
    }
}
