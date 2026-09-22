namespace EmotionService.Contracts.MediaDetails;

public sealed record BookDetailRequest(
    string Author,
    string Publisher,
    DateOnly? PublicationDate,
    string Genre,
    string ISBN,
    int? PageCount,
    string Language,
    string Description,
    string? Edition);
