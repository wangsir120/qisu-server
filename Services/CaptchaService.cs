namespace qisu_server.Services;

public interface ICaptchaService
{
    (string captchaId, string captchaText) GenerateCaptcha();
    bool ValidateCaptcha(string captchaId, string inputCode);
}

public class CaptchaService : ICaptchaService
{
    private static readonly Dictionary<string, (string code, DateTime expireTime)> _captchaStore = new();
    private static readonly object _lock = new();
    private const int CaptchaLength = 4;
    private const int ExpireMinutes = 5;

    public (string captchaId, string captchaText) GenerateCaptcha()
    {
        var captchaId = Guid.NewGuid().ToString("N");
        var captchaText = GenerateRandomCode(CaptchaLength);

        lock (_lock)
        {
            CleanExpiredCaptcha();
            _captchaStore[captchaId] = (captchaText, DateTime.Now.AddMinutes(ExpireMinutes));
        }

        return (captchaId, captchaText);
    }

    public bool ValidateCaptcha(string captchaId, string inputCode)
    {
        if (string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(inputCode))
            return false;

        lock (_lock)
        {
            if (!_captchaStore.TryGetValue(captchaId, out var stored))
                return false;

            _captchaStore.Remove(captchaId);

            if (DateTime.Now > stored.expireTime)
                return false;

            return string.Equals(stored.code, inputCode, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static void CleanExpiredCaptcha()
    {
        var expiredKeys = _captchaStore
            .Where(x => DateTime.Now > x.Value.expireTime)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _captchaStore.Remove(key);
        }
    }
}
