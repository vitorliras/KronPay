using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Shouldly;

namespace Tests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Should_create_user_with_valid_data()
    {
        var email = new Email("user@email.com");

        var user = new UserValueObject("Vitor", email);

        user.Name.ShouldBe("Vitor");
        user.Email.Address.ShouldBe("user@email.com");
    }

    [Fact]
    public void Should_not_allow_empty_name()
    {
        var email = new Email("user@email.com");

        Should.Throw<DomainException>(() =>
        {
            new UserValueObject("", email);
        });
    }
}
