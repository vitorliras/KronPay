using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.UseCases.Auth;
using KronPay.Domain.Entities.Users;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.ValueObjects;
using Domain.ValueObjects.User;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class CreateUserUseCase
    : IUseCase<CreateUserRequest, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;
    private readonly SendEmailConfirmationCodeUseCase _sendConfirmationCodeUseCase;
    private readonly ILogger<CreateUserUseCase> _logger;

    public CreateUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow,
        SendEmailConfirmationCodeUseCase sendConfirmationCodeUseCase,
        ILogger<CreateUserUseCase> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
        _sendConfirmationCodeUseCase = sendConfirmationCodeUseCase;
        _logger = logger;
    }

    public async Task<ResultEntity<UserResponse>> ExecuteAsync(CreateUserRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return ResultEntity<UserResponse>.Failure(MessageKeys.EmailAlreadyExists);
            }

            existingUser = await _userRepository.GetByCpfAsync(request.Cpf);
            if (existingUser is not null)
            {
                return ResultEntity<UserResponse>.Failure( MessageKeys.CpfAlreadyExists);
            }

            existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser is not null)
            {
                return ResultEntity<UserResponse>.Failure( MessageKeys.UsernameAlreadyExists);
            }

            var name = Name.Create(request.Name);
            var userName = Name.Create(request.Username);
            var email =  new Email(request.Email);
            var cpf = new Cpf(request.Cpf);
            var phone = new Phone(request.Phone);

            var password = new Password(request.Password);

            var passwordHash = _passwordHasher.Hash(password.Value);

            var user = new User(
                name: name,
                username: userName,
                email: email,
                cpf: cpf,
                phone: phone,
                passwordHash: passwordHash,
                userType: "B"
            );

            var result = await _userRepository.AddAsync(user);
            if (!result)
                return ResultEntity<UserResponse>.Failure(MessageKeys.InsertFalied);

            var uow = await _uow.CommitAsync();
            if (!uow)
                return ResultEntity<UserResponse>.Failure(MessageKeys.DataPersistenceFailed);

            await SendConfirmationCodeBestEffort(user.Id, user.Email.Value);

            return ResultEntity<UserResponse>.Success(
                new UserResponse(
                    user.Id,
                    user.Name.Value,
                    user.Username.Value,
                    user.Email.Value

                ), MessageKeys.OperationSuccess
            );
        }
        catch (Exception e)
        {
            return ResultEntity<UserResponse>.Failure( e.Message);
        }

    }

    private async Task SendConfirmationCodeBestEffort(int userId, string email)
    {
        try
        {
            var result = await _sendConfirmationCodeUseCase.ExecuteAsync(
                new SendEmailConfirmationCodeRequest(userId, email));

            if (!result.IsSuccess)
                _logger.LogError(
                    "Não foi possível enviar o código de confirmação ao criar o usuário {UserId}: {Message}",
                    userId, result.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao disparar o código de confirmação para o usuário {UserId}.", userId);
        }
    }
}
