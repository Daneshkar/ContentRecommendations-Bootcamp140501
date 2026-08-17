namespace AuthService.Domain.Base;

public interface IAuditable
{
    DateTime  CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
