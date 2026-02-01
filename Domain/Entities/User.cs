using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.User;
using System.Xml.Linq;

public sealed class User
{
    public int Id { get; private set; }
    public Name Name { get; private set; }
    public Name Username { get; private set; }
    public Email Email { get; private set; }

    public Phone Phone { get; private set; }
    public Cpf Cpf { get; private set; }
    public string PasswordHash { get; private set; }

    public UserType UserType { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastAccessAt { get; private set; }

    protected User() { }

    public User(
     Name name,
     Name username,
     Email email,
     Cpf cpf,
     Phone phone,
     string passwordHash,
     UserType userType)
    {
        Name = name;
        Username = username;
        Email = email;
        Cpf = cpf;
        Phone = phone;
        PasswordHash = passwordHash;
        UserType = userType;
        CreatedAt = DateTime.UtcNow;
    }

    public void RegisterAccess()
    {
        LastAccessAt = DateTime.UtcNow;
    }
}
