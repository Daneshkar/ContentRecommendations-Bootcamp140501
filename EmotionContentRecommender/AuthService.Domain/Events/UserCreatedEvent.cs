using AuthService.Domain.Base;

namespace AuthService.Domain.Events;

/// <summary>
/// هنگام ثبت‌نام موفق کاربر جدید raise می‌شود
/// می‌توان برای ارسال ایمیل خوشامدگویی، لاگ و ... استفاده کرد
/// </summary>
public record UserCreatedEvent(string Username) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
