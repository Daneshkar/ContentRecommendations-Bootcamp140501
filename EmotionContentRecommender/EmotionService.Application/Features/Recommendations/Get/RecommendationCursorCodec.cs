using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EmotionService.Infrastructure.Exceptions;

namespace EmotionService.Application.Features.Recommendations.Get;

internal sealed record RecommendationCursor(
    string CriteriaFingerprint,
    decimal FinalScore,
    decimal PrimaryMoodScore,
    int PrimaryExperienceCount,
    string MediaItemKey);

internal sealed class RecommendationCursorCodec
{
    private const byte FormatVersion = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _encryptionKey;

    public RecommendationCursorCodec(string secret)
    {
        _encryptionKey = SHA256.HashData(
            Encoding.UTF8.GetBytes(
                $"EmotionService.RecommendationCursor.v1:{secret}"));
    }

    public string Encode(RecommendationCursor cursor)
    {
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(cursor);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
        payload[0] = FormatVersion;
        nonce.CopyTo(payload.AsSpan(1, NonceSize));
        tag.CopyTo(payload.AsSpan(1 + NonceSize, TagSize));
        ciphertext.CopyTo(payload.AsSpan(1 + NonceSize + TagSize));

        return ToBase64Url(payload);
    }

    public RecommendationCursor Decode(string encodedCursor)
    {
        try
        {
            var payload = FromBase64Url(encodedCursor);

            if (payload.Length <= 1 + NonceSize + TagSize
                || payload[0] != FormatVersion)
            {
                throw new FormatException();
            }

            var nonce = payload.AsSpan(1, NonceSize);
            var tag = payload.AsSpan(1 + NonceSize, TagSize);
            var ciphertext = payload.AsSpan(1 + NonceSize + TagSize);
            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_encryptionKey, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return JsonSerializer.Deserialize<RecommendationCursor>(plaintext)
                ?? throw new FormatException();
        }
        catch (Exception exception) when (
            exception is FormatException
                or CryptographicException
                or JsonException)
        {
            throw new BadRequestException(
                "نشانگر صفحه‌بندی نامعتبر یا منقضی است.");
        }
    }

    public static string CreateCriteriaFingerprint(
        long userId,
        int itemTypeId,
        int primaryMoodId,
        IReadOnlyCollection<int> additionalMoodIds,
        IReadOnlyCollection<int> themeIds)
    {
        var criteria = string.Join(
            '|',
            userId,
            itemTypeId,
            primaryMoodId,
            string.Join(',', additionalMoodIds.Order()),
            string.Join(',', themeIds.Order()));

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(criteria)));
    }

    private static string ToBase64Url(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        base64 = (base64.Length % 4) switch
        {
            0 => base64,
            2 => base64 + "==",
            3 => base64 + "=",
            _ => throw new FormatException()
        };

        return Convert.FromBase64String(base64);
    }
}
