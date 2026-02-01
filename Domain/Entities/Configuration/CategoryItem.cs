using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities;

public sealed class CategoryItem
{
    public int Id { get; private set; }
    public int CategoryId { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    protected CategoryItem() { } 

    public CategoryItem(
        int categoryId,
        int userId,
        string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        CategoryId = categoryId;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
        Active = true;
        DeactivatedAt = null;
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
