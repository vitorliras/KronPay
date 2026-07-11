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
    }

    [Fact]
    public void Should_not_allow_empty_name()
    {
    }
}
