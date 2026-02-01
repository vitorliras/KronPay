using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.DTOs.Configuration.Categories;
using Application.DTOs.Users;
using Domain.Enums;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.ValueObjects;
using Domain.ValueObjects.User;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Users;

public sealed class CreateUserUseCase
    : IUseCase<CreateUserRequest, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public CreateUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task<ResultT<UserResponse>> ExecuteAsync(CreateUserRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return ResultT<UserResponse>.Failure("",MessageKeys.UserAlreadyExists);
            }

            existingUser = await _userRepository.GetByCpfAsync(request.Cpf);
            if (existingUser is not null)
            {
                return ResultT<UserResponse>.Failure("", MessageKeys.UserAlreadyExists);
            }

            existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser is not null)
            {
                return ResultT<UserResponse>.Failure("", MessageKeys.UserAlreadyExists);
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
                userType: UserType.Default
            );

            var result = await _userRepository.AddAsync(user);
            if (!result)
                return ResultT<UserResponse>.Failure(MessageKeys.OperationFailed);

            var uow = await _uow.CommitAsync();
            if (!uow)
                return ResultT<UserResponse>.Failure(MessageKeys.OperationFailed);


            return ResultT<UserResponse>.Success(
                new UserResponse(
                    user.Id,
                    user.Name.Value,
                    user.Username.Value,
                    user.Email.Value

                )
            );
        }
        catch (Exception e)
        {
            return ResultT<UserResponse>.Failure("", e.Message);
        }
       
    }
}
