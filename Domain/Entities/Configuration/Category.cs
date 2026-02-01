using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Configuration;

public sealed class Category
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Description { get; private set; }
    public string CodTypeTransaction { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; private set; }
    public DateTime ? DeactivatedAt { get; private set; }

    protected Category() { } 

    public Category(
        int userId,
        string description,
        string codTypeTransaction)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        UserId = userId;
        Description = description.Trim();
        CodTypeTransaction = codTypeTransaction;
        CreatedAt = DateTime.UtcNow;
        DeactivatedAt = null;
        Active = true; 
    }

    public void Deactivate()
    {
        Active = false;
        DeactivatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        Description = description.Trim();
    }
}
