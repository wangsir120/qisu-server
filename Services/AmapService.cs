using System.Text.Json;
using Microsoft.Extensions.Options;

namespace qisu_server.Services;

public class AmapConfig
{
    public string WebApiKey { get; set; } = string.Empty;
}

public class AmapService
{
    private readonly HttpClient _http;
    private readonly AmapConfig _config;
    private readonly ILogger<AmapService> _logger;

    public AmapService(HttpClient http, IOptions<AmapConfig> config, ILogger<AmapService> logger)
    {
        _http = http;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<List<AmapPoiItem>> GetHotDestinationsAsync(string city = "重庆", int limit = 20)
    {
        var key = _config.WebApiKey;
        if (string.IsNullOrWhiteSpace(key) || key == "YOUR_AMAP_WEB_API_KEY")
        {
            _logger.LogWarning("高德Web API Key未配置，请在 appsettings.json 的 Amap.WebApiKey 填入真实的Key（高德开放平台 → 应用管理 → Web服务Key）");
            return new List<AmapPoiItem>();
        }

        var results = new List<AmapPoiItem>();
        var keywordsList = new[] {
            "热门景点", "必游景点", "网红打卡地", "名胜古迹",
            "公园广场", "博物馆", "古镇", "商圈"
        };
        var seen = new HashSet<string>();

        foreach (var keywords in keywordsList)
        {
            try
            {
                var url = $"https://restapi.amap.com/v3/place/text?keywords={Uri.EscapeDataString(keywords)}&city={Uri.EscapeDataString(city)}&types=风景名胜|公园广场|博物馆|商业区&offset=10&page=1&key={key}&extensions=base";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("高德API HTTP {Code}: keywords={Keywords}, body={Body}", (int)response.StatusCode, keywords, body);
                    continue;
                }

                var result = JsonSerializer.Deserialize<AmapPoiResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || result.Status != "1")
                {
                    _logger.LogWarning("高德API返回异常: keywords={Keywords}, status={Status}, info={Info}", keywords, result?.Status, result?.Info);
                    continue;
                }

                if (result.Pois == null) continue;

                foreach (var poi in result.Pois)
                {
                    if (string.IsNullOrWhiteSpace(poi.Name)) continue;
                    var cleanName = CleanPoiName(poi.Name);
                    if (string.IsNullOrWhiteSpace(cleanName) || cleanName.Length < 2) continue;

                    if (seen.Add(cleanName))
                    {
                        poi.Name = cleanName;
                        results.Add(poi);
                    }
                }

                _logger.LogInformation("高德POI搜索成功: keywords={Keywords}, 获取{Count}条", keywords, result.Pois.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "高德POI搜索异常: keywords={Keywords}", keywords);
            }
        }

        return results.OrderByDescending(r => r.BizExt?.Rating ?? 0).Take(limit).ToList();
    }

    private static string CleanPoiName(string name)
    {
        name = name.Trim();
        if (name.StartsWith("重庆市")) name = name[3..].Trim();
        if (name.EndsWith("（景区）")) name = name[..^4].Trim();
        if (name.EndsWith("景区")) name = name[..^2].Trim();
        if (name.EndsWith("公园")) name = name.Trim();
        return name;
    }
}

public class AmapPoiResponse
{
    public string Status { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
    public string Count { get; set; } = string.Empty;
    public List<AmapPoiItem>? Pois { get; set; }
}

public class AmapPoiItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Pname { get; set; } = string.Empty;
    public string Cityname { get; set; } = string.Empty;
    public string Adname { get; set; } = string.Empty;
    public List<AmapPhoto>? Photos { get; set; }
    public AmapBizExt? BizExt { get; set; }
}

public class AmapPhoto
{
    public string Url { get; set; } = string.Empty;
}

public class AmapBizExt
{
    public decimal Rating { get; set; }
    public decimal Cost { get; set; }
}
