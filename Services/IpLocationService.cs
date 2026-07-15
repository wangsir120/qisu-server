using System.Net;
using System.Text.Json;

namespace qisu_server.Services;

public interface IIpLocationService
{
    Task<string?> GetLocationAsync(string ipAddress);
    Task<(string? Ip, string? Location)> GetLocationWithIpAsync(string ipAddress);
}

public class IpLocationService : IIpLocationService
{
    private readonly ILogger<IpLocationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly Dictionary<string, string> _cache = new();
    private static readonly object _cacheLock = new();

    public IpLocationService(ILogger<IpLocationService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public static string NormalizeIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return "unknown";
        }

        ipAddress = ipAddress.Trim();

        if (ipAddress == "::1")
        {
            return "127.0.0.1";
        }

        if (IPAddress.TryParse(ipAddress, out var address))
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv4MappedToIPv6)
                {
                    return address.MapToIPv4().ToString();
                }
                return ipAddress;
            }
        }

        return ipAddress;
    }

    public static bool IsLocalIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return false;
        }

        ipAddress = NormalizeIpAddress(ipAddress);

        if (ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress == "localhost" || ipAddress == "unknown")
        {
            return true;
        }

        if (ipAddress.StartsWith("192.168.") || ipAddress.StartsWith("10."))
        {
            return true;
        }

        if (ipAddress.StartsWith("172."))
        {
            var parts = ipAddress.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[1], out var second))
            {
                if (second >= 16 && second <= 31)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsValidPublicIpAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip))
        {
            return false;
        }

        if (!IPAddress.TryParse(ip, out var address))
        {
            return false;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return false;
        }

        return !IsLocalIpAddress(ip);
    }

    public async Task<string?> GetLocationAsync(string ipAddress)
    {
        ipAddress = NormalizeIpAddress(ipAddress);

        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "unknown")
        {
            return null;
        }

        if (IsLocalIpAddress(ipAddress))
        {
            return "本地网络";
        }

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(ipAddress, out var cachedLocation))
            {
                return cachedLocation;
            }
        }

        var location = await GetLocationFromMultipleSourcesAsync(ipAddress);
        
        if (!string.IsNullOrEmpty(location))
        {
            lock (_cacheLock)
            {
                _cache[ipAddress] = location;
            }
        }

        return location;
    }

    public async Task<(string? Ip, string? Location)> GetLocationWithIpAsync(string ipAddress)
    {
        ipAddress = NormalizeIpAddress(ipAddress);

        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "unknown")
        {
            _logger.LogWarning("IP地址为空或unknown");
            return (null, null);
        }

        _logger.LogInformation("处理IP地址: {IpAddress}", ipAddress);

        if (IsLocalIpAddress(ipAddress))
        {
            _logger.LogInformation("本地/内网IP地址，标记为本地网络: {IpAddress}", ipAddress);
            return (ipAddress, "本地网络");
        }

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(ipAddress, out var cachedLocation))
            {
                _logger.LogInformation("从缓存获取地理位置: {IpAddress} -> {Location}", ipAddress, cachedLocation);
                return (ipAddress, cachedLocation);
            }
        }

        var location = await GetLocationFromMultipleSourcesAsync(ipAddress);

        if (!string.IsNullOrEmpty(location))
        {
            lock (_cacheLock)
            {
                _cache[ipAddress] = location;
            }
        }

        _logger.LogInformation("成功获取地理位置: {IpAddress} -> {Location}", ipAddress, location);
        return (ipAddress, location);
    }

    private async Task<string?> GetLocationFromMultipleSourcesAsync(string ipAddress)
    {
        var location = await TryGetLocationFromIpApiAsync(ipAddress);
        if (!string.IsNullOrEmpty(location))
        {
            return location;
        }

        location = await TryGetLocationFromIpInfoAsync(ipAddress);
        if (!string.IsNullOrEmpty(location))
        {
            return location;
        }

        return null;
    }

    private async Task<string?> TryGetLocationFromIpApiAsync(string ipAddress)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetStringAsync($"http://ip-api.com/json/{ipAddress}?lang=zh-CN&fields=status,country,regionName,city,district,isp,query");
            _logger.LogInformation("ip-api.com 响应: {Response}", response);
            
            var json = JsonSerializer.Deserialize<IpApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (json?.Status == "success")
            {
                var location = new List<string>();
                if (!string.IsNullOrEmpty(json.Country))
                {
                    location.Add(json.Country);
                }
                if (!string.IsNullOrEmpty(json.RegionName) && json.RegionName != json.Country)
                {
                    location.Add(json.RegionName);
                }
                if (!string.IsNullOrEmpty(json.City) && json.City != json.RegionName)
                {
                    location.Add(json.City);
                }
                if (!string.IsNullOrEmpty(json.District))
                {
                    location.Add(json.District);
                }

                return location.Count > 0 ? string.Join(" ", location) : null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从 ip-api.com 获取IP地理位置失败: {IpAddress}", ipAddress);
        }

        return null;
    }

    private async Task<string?> TryGetLocationFromIpInfoAsync(string ipAddress)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetStringAsync($"https://ipinfo.io/{ipAddress}/json");
            _logger.LogInformation("ipinfo.io 响应: {Response}", response);
            
            var json = JsonSerializer.Deserialize<IpInfoResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (json != null && !string.IsNullOrEmpty(json.Country))
            {
                var location = new List<string>();
                if (!string.IsNullOrEmpty(json.Country))
                {
                    var countryName = GetCountryName(json.Country);
                    if (!string.IsNullOrEmpty(countryName))
                    {
                        location.Add(countryName);
                    }
                }
                if (!string.IsNullOrEmpty(json.Region))
                {
                    location.Add(json.Region);
                }
                if (!string.IsNullOrEmpty(json.City))
                {
                    location.Add(json.City);
                }

                return location.Count > 0 ? string.Join(" ", location) : null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从 ipinfo.io 获取IP地理位置失败: {IpAddress}", ipAddress);
        }

        return null;
    }

    private string? GetCountryName(string countryCode)
    {
        var countries = new Dictionary<string, string>
        {
            { "CN", "中国" },
            { "US", "美国" },
            { "JP", "日本" },
            { "KR", "韩国" },
            { "GB", "英国" },
            { "DE", "德国" },
            { "FR", "法国" },
            { "AU", "澳大利亚" },
            { "CA", "加拿大" },
            { "RU", "俄罗斯" },
            { "SG", "新加坡" },
            { "HK", "香港" },
            { "TW", "台湾" },
            { "MO", "澳门" }
        };

        return countries.TryGetValue(countryCode, out var name) ? name : countryCode;
    }

    private class IpApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Isp { get; set; }
        public string? Message { get; set; }
    }

    private class IpInfoResponse
    {
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? Org { get; set; }
    }
}
