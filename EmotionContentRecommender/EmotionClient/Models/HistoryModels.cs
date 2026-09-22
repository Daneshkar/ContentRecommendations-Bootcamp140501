namespace EmotionClient.Models;

public sealed class ExperienceHistoryPageViewModel
{
    public IReadOnlyList<UserExperienceSummary> Experiences { get; init; } = [];
    public string? ErrorMessage { get; init; }

    public int HighlyRatedCount => Experiences.Count(item => item.Score >= 4);

    public int ExploredMoodCount => Experiences
        .SelectMany(item => item.Moods)
        .Select(item => item.Id)
        .Distinct()
        .Count();
}

public sealed class ExperienceHistoryTag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
