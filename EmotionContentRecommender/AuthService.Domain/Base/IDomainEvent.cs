using MediatR;

namespace AuthService.Domain.Base;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
