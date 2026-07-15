using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using qisu_server.Data;
using qisu_server.Models;

namespace qisu_server.Services;

public class HotDestinationUpdateService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AmapService _amapService;
    private readonly ILogger<HotDestinationUpdateService> _logger;

    public HotDestinationUpdateService(
        IServiceScopeFactory scopeFactory,
        AmapService amapService,
        ILogger<HotDestinationUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _amapService = amapService;
        _logger = logger;
    }

    public async Task UpdateHotDestinationsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            _logger.LogInformation("开始更新热门目的地...");

            var poiList = await _amapService.GetHotDestinationsAsync("重庆", 6);

            if (poiList.Count == 0)
            {
                _logger.LogWarning("高德API未返回目的地数据，使用现有地址数据兜底刷新");
                await RefreshFromAddressesAsync(context);
                return;
            }

            var existingDestinations = await context.HotDestinations.ToListAsync();
            var existingNames = existingDestinations.Select(d => d.Name ?? "").ToHashSet();

            var allProperties = await context.Properties
                .Include(p => p.PropertyAddress)
                .Where(p => p.Status == 1)
                .ToListAsync();

            var sortOrder = 0;
            foreach (var poi in poiList)
            {
                sortOrder++;

                var propertyCount = allProperties.Count(p =>
                    p.Title.Contains(poi.Name) ||
                    (p.PropertyAddress?.FullAddress != null && p.PropertyAddress.FullAddress.Contains(poi.Name)));

                if (propertyCount == 0) continue;

                var imageUrl = poi.Photos?.FirstOrDefault()?.Url;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = $"https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt={System.Web.HttpUtility.UrlEncode($"{poi.Name} chongqing china travel destination scenic cityscape")}&image_size=square";
                }

                var existing = existingDestinations.FirstOrDefault(d => d.Name == poi.Name);
                if (existing != null)
                {
                    existing.PropertyCount = propertyCount;
                    existing.Image = imageUrl;
                    existing.SortOrder = sortOrder;
                    existing.HotScore = propertyCount * 10m;
                    existing.UpdatedAt = DateTime.Now;
                    existing.LastUpdatedBy = "amap";
                    existing.Status = true;
                }
                else if (!existingNames.Contains(poi.Name))
                {
                    context.HotDestinations.Add(new HotDestination
                    {
                        Name = poi.Name,
                        Image = imageUrl,
                        PropertyCount = propertyCount,
                        SortOrder = sortOrder,
                        Status = true,
                        SearchCount = 0,
                        BookingCount = 0,
                        ViewCount = 0,
                        HotScore = propertyCount * 10m,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        LastUpdatedBy = "amap"
                    });
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("热门目的地更新完成，高德返回 {PoiCount} 条，有效匹配 {Added} 条", poiList.Count, sortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新热门目的地失败");
            throw;
        }
    }

    private async Task RefreshFromAddressesAsync(AppDbContext context)
    {
        var stats = await (from a in context.Addresses
                          join p in context.Properties on a.Id equals p.AddressId
                          where p.Status == 1
                          group p by a.District into g
                          where g.Key != null && g.Count() > 0
                          let cnt = g.Count()
                          orderby cnt descending
                          select new { Name = g.Key, Count = cnt })
            .ToListAsync();

        var existingDict = (await context.HotDestinations.ToListAsync())
            .ToDictionary(d => d.Name ?? "", d => d);

        int sortOrder = 0;
        foreach (var item in stats)
        {
            sortOrder++;
            var district = item.Name ?? "";
            if (existingDict.TryGetValue(district, out var dest))
            {
                dest.PropertyCount = item.Count;
                dest.SortOrder = sortOrder;
                dest.HotScore = item.Count * 10m;
                dest.UpdatedAt = DateTime.Now;
                dest.LastUpdatedBy = "fallback";
                dest.Status = true;
            }
            else
            {
                context.HotDestinations.Add(new HotDestination
                {
                    Name = district,
                    Image = $"https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt={System.Web.HttpUtility.UrlEncode($"重庆{district} chongqing china travel destination scenic cityscape")}&image_size=square",
                    PropertyCount = item.Count,
                    SortOrder = sortOrder,
                    Status = true,
                    HotScore = item.Count * 10m,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    LastUpdatedBy = "fallback"
                });
            }
        }

        foreach (var (name, dest) in existingDict)
        {
            dest.Status = stats.Any(s => s.Name == name);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("兜底刷新完成，共 {Count} 个目的地", stats.Count);
    }
}
