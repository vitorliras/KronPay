using Domain.Entities.Notifications;

namespace Application.Notifications;

public interface INotificationEmailDispatcher
{
    Task DispatchAsync(Notification notification, int userId);
}
