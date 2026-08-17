using AuthService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Events;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "کاربر جدید با نام کاربری {Username} در تاریخ {OccurredOn} ثبت‌نام کرد",
            notification.Username,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
