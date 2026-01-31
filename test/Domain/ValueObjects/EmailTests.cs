using Domain.Exceptions;
using Domain.ValueObjects;
using Shouldly;

namespace Tests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Should_create_valid_email()
    {
        var email = new Email("teste@ema0il.com");

        email.Address.ShouldBe("teste@email.com");
    }

    [Fact]
    public void Should_throw_exception_for_invalid_email()
    {
        Should.Throw<DomainException>(() =>
        {
            new Email("email_invalido");
        });
    }
}
