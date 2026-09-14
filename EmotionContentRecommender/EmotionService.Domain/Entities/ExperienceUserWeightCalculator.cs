namespace EmotionService.Domain.Entities;

public static class ExperienceUserWeightCalculator
{
    private const decimal NeutralWeight = 0.05m;
    private const decimal ScoreStep = 0.02m;

    public static decimal FromScore(int score)
    {
        if (score is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Score must be between 1 and 5.");
        }

        return NeutralWeight + ScoreStep * (score - 3);
    }
}
