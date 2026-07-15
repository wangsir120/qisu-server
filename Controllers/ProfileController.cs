using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProfileController> _logger;
    private readonly IIdCardService? _idCardService;
    private readonly NotificationService _notificationService;

    public ProfileController(AppDbContext context, ILogger<ProfileController> logger, IIdCardService? idCardService = null, NotificationService notificationService = null)
    {
        _context = context;
        _logger = logger;
        _idCardService = idCardService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// 获取当前登录用户的详细信息
    /// </summary>
    /// <returns>用户详细信息</returns>
    [HttpGet("info")]
    public async Task<ApiResponse<UserProfileDto>> GetUserInfo()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<UserProfileDto>.Fail("用户未登录");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse<UserProfileDto>.Fail("用户不存在");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        var role = host != null && host.Status == 1 ? "landlord" : "user";

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Phone = user.Phone,
            Email = user.Email,
            Gender = user.Gender,
            IdCard = user.IdCard,
            IsVerified = user.IsVerified,
            Role = role,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    /// <summary>
    /// 更新当前登录用户的个人信息
    /// </summary>
    /// <param name="request">更新的用户信息</param>
    /// <returns>更新后的用户信息</returns>
    [HttpPut("info")]
    public async Task<ApiResponse<UserProfileDto>> UpdateUserInfo([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<UserProfileDto>.Fail("用户未登录");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return ApiResponse<UserProfileDto>.Fail("用户不存在");
            }

            if (!string.IsNullOrEmpty(request.Nickname))
            {
                user.Nickname = request.Nickname;
            }

            if (!string.IsNullOrEmpty(request.Gender))
            {
                if (request.Gender != "male" && request.Gender != "female" && request.Gender != "other")
                {
                    return ApiResponse<UserProfileDto>.Fail("性别值无效，应为：male、female、other");
                }
                user.Gender = request.Gender;
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                user.Phone = request.Phone;
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
            }

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 更新个人信息成功", userId);

            return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender,
                IdCard = user.IdCard,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            }, "更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户信息失败");
            return ApiResponse<UserProfileDto>.Fail("更新失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 实名认证（身份证二要素验证）
    /// </summary>
    /// <param name="request">实名认证请求，包含姓名和身份证号</param>
    /// <returns>实名认证结果</returns>
    [HttpPost("verify-idcard")]
    public async Task<ApiResponse<UserProfileDto>> VerifyIdCard([FromBody] VerifyIdCardRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<UserProfileDto>.Fail("用户未登录");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return ApiResponse<UserProfileDto>.Fail("用户不存在");
            }

            if (user.IsVerified)
            {
                return ApiResponse<UserProfileDto>.Fail("您已完成实名认证");
            }

            if (_idCardService == null)
            {
                return ApiResponse<UserProfileDto>.Fail("实名认证服务未配置");
            }

            if (string.IsNullOrEmpty(request.RealName))
            {
                return ApiResponse<UserProfileDto>.Fail("姓名不能为空");
            }

            if (string.IsNullOrEmpty(request.IdCard))
            {
                return ApiResponse<UserProfileDto>.Fail("身份证号不能为空");
            }

            if (request.IdCard.Length != 15 && request.IdCard.Length != 18)
            {
                return ApiResponse<UserProfileDto>.Fail("身份证号格式不正确");
            }

            var verifyResult = await _idCardService.VerifyAsync(request.RealName, request.IdCard);

            if (!verifyResult.Success || !verifyResult.Data?.IsMatch == true)
            {
                return ApiResponse<UserProfileDto>.Fail("姓名和身份证号不匹配");
            }

            user.IdCard = request.IdCard;
            user.IsVerified = true;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 实名认证成功", userId);

            return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender,
                IdCard = user.IdCard,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            }, "实名认证成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "实名认证失败");
            return ApiResponse<UserProfileDto>.Fail("实名认证失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 更新用户头像
    /// </summary>
    /// <param name="request">头像URL</param>
    /// <returns>更新结果</returns>
    [HttpPut("avatar")]
    public async Task<ApiResponse<bool>> UpdateAvatar([FromBody] UpdateUserAvatarRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("用户不存在");
        }

        if (string.IsNullOrEmpty(request.Avatar))
        {
            return ApiResponse<bool>.Fail("头像地址不能为空");
        }

        user.Avatar = request.Avatar;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("用户 {UserId} 更新头像成功", userId);

        return ApiResponse<bool>.Ok(true, "头像更新成功");
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="request">包含旧密码和新密码</param>
    /// <returns>修改结果</returns>
    [HttpPut("password")]
    public async Task<ApiResponse<bool>> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var role = GetCurrentRole();

        var admin = await _context.Admins.FindAsync(userId.Value);
        var user = await _context.Users.FindAsync(userId.Value);

        string? currentPassword = null;
        string? entityType = null;

        if (admin != null)
        {
            currentPassword = admin.Password;
            entityType = "admin";
        }
        else if (user != null)
        {
            currentPassword = user.Password;
            entityType = "user";
        }
        else
        {
            return ApiResponse<bool>.Fail("用户不存在");
        }

        if (string.IsNullOrEmpty(request.OldPassword))
        {
            return ApiResponse<bool>.Fail("请输入原密码");
        }

        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return ApiResponse<bool>.Fail("请输入新密码");
        }

        if (request.NewPassword.Length < 6)
        {
            return ApiResponse<bool>.Fail("新密码至少6个字符");
        }

        if (request.OldPassword != currentPassword)
        {
            return ApiResponse<bool>.Fail("原密码错误");
        }

        if (entityType == "admin")
        {
            admin!.Password = request.NewPassword;
            admin.UpdatedAt = DateTime.Now;
        }
        else
        {
            user!.Password = request.NewPassword;
            user.UpdatedAt = DateTime.Now;
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("{EntityType} {UserId} 修改密码成功", entityType, userId);

        return ApiResponse<bool>.Ok(true, "密码修改成功");
    }

    /// <summary>
    /// 绑定手机号
    /// </summary>
    /// <param name="request">包含手机号和验证码</param>
    /// <returns>绑定结果</returns>
    [HttpPost("bind-phone")]
    public async Task<ApiResponse<bool>> BindPhone([FromBody] BindPhoneRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("用户不存在");
        }

        if (string.IsNullOrEmpty(request.Phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        if (string.IsNullOrEmpty(request.VerifyCode))
        {
            return ApiResponse<bool>.Fail("请输入验证码");
        }

        // 检查手机号是否已被其他用户绑定
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone && u.Id != userId.Value);
        if (existingUser != null)
        {
            return ApiResponse<bool>.Fail("该手机号已被其他账号绑定");
        }

        // 验证短信验证码（需要注入 ISmsService）
        // 这里简化处理，实际应该调用 SmsService 验证
        // var smsService = HttpContext.RequestServices.GetRequiredService<ISmsService>();
        // var (verifySuccess, verifyMessage) = await smsService.VerifyCodeAsync(request.Phone, request.VerifyCode);
        // if (!verifySuccess) return ApiResponse<bool>.Fail(verifyMessage);

        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("用户 {UserId} 绑定手机号成功: {Phone}", userId, request.Phone);

        return ApiResponse<bool>.Ok(true, "手机号绑定成功");
    }

    /// <summary>
    /// 获取当前用户的收藏列表
    /// </summary>
    /// <returns>收藏列表</returns>
    [HttpGet("favorites")]
    public async Task<ApiResponse<List<FavoriteItemDto>>> GetFavorites()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<List<FavoriteItemDto>>.Fail("用户未登录");
        }

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Property)
            .ThenInclude(p => p.Images)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var result = favorites.Select(f =>
        {
            var tags = new List<string>();
            if (f.Property?.IsInstantBook == true) tags.Add("闪订");
            if (f.Property?.IsNew == true) tags.Add("新上线");
            if (f.Property?.Rating >= 4.8m) tags.Add("高分房源");

            return new FavoriteItemDto
            {
                Id = f.Id,
                PropertyId = f.PropertyId,
                Title = f.Property?.Title ?? "",
                CoverImage = f.Property?.Images != null && f.Property.Images.Any()
                    ? (f.Property.Images.FirstOrDefault(i => i.IsCover) != null
                        ? f.Property.Images.FirstOrDefault(i => i.IsCover)!.ImageUrl
                        : f.Property.Images.First().ImageUrl)
                    : null,
                Address = f.Property?.PropertyAddress?.FullAddress ?? "",
                Price = f.Property?.PricePerNight ?? 0,
                Rating = (double)(f.Property?.Rating ?? 0),
                ReviewCount = f.Property?.ReviewCount ?? 0,
                Tags = tags,
                CreatedAt = f.CreatedAt
            };
        }).ToList();

        return ApiResponse<List<FavoriteItemDto>>.Ok(result);
    }

    /// <summary>
    /// 添加收藏
    /// </summary>
    /// <param name="request">收藏请求</param>
    /// <returns>收藏结果</returns>
    [HttpPost("favorites")]
    public async Task<ApiResponse<bool>> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var property = await _context.Properties.FindAsync(request.PropertyId);
        if (property == null)
        {
            return ApiResponse<bool>.Fail("房源不存在");
        }

        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == request.PropertyId);

        if (existingFavorite != null)
        {
            return ApiResponse<bool>.Fail("已收藏该房源");
        }

        var favorite = new Favorite
        {
            UserId = userId.Value,
            PropertyId = request.PropertyId,
            CreatedAt = DateTime.Now
        };

        _context.Favorites.Add(favorite);
        property.FavoriteCount = property.FavoriteCount + 1;
        await _context.SaveChangesAsync();

        _logger.LogInformation("用户 {UserId} 收藏房源 {PropertyId} 成功", userId, request.PropertyId);

        return ApiResponse<bool>.Ok(true, "收藏成功");
    }

    /// <summary>
    /// 取消收藏
    /// </summary>
    /// <param name="propertyId">房源ID</param>
    /// <returns>取消结果</returns>
    [HttpDelete("favorites/{propertyId}")]
    public async Task<ApiResponse<bool>> RemoveFavorite(long propertyId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

        if (favorite == null)
        {
            return ApiResponse<bool>.Fail("未收藏该房源");
        }

        var property = await _context.Properties.FindAsync(propertyId);
        if (property != null)
        {
            property.FavoriteCount = Math.Max(0, property.FavoriteCount - 1);
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();

        _logger.LogInformation("用户 {UserId} 取消收藏房源 {PropertyId}", userId, propertyId);

        return ApiResponse<bool>.Ok(true, "取消收藏成功");
    }

    /// <summary>
    /// 检查房源是否已收藏
    /// </summary>
    /// <param name="propertyId">房源ID</param>
    /// <returns>检查结果</returns>
    [HttpGet("favorites/check/{propertyId}")]
    public async Task<ApiResponse<bool>> CheckFavorite(long propertyId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Ok(false);
        }

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);

        return ApiResponse<bool>.Ok(exists);
    }

    /// <summary>
    /// 获取当前用户的订单列表（管理员可查看所有订单）
    /// </summary>
    /// <param name="status">订单状态筛选（可选）：pending, paid, confirmed, staying, completed, cancelled, refunded</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>订单列表</returns>

    [HttpGet("orders/{orderId}")]
    public async Task<ApiResponse<object>> GetOrderById(long orderId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<object>.Fail("用户未登录");
        }

        try
        {
            var role = GetCurrentRole();
            var isAdmin = role == "admin" || role == "super_admin";

            var order = await _context.Orders
                .Include(o => o.Property)
                .ThenInclude(p => p.Images)
                .Include(o => o.Property)
                .ThenInclude(p => p.PropertyAddress)
                .Include(o => o.Host)
                .Include(o => o.User)
                .Include(o => o.Room)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return ApiResponse<object>.Fail("订单不存在");
            }

            if (!isAdmin && order.UserId != userId && !(order.HostId > 0 && _context.Hosts.Any(h => h.Id == order.HostId && h.UserId == userId)))
            {
                return ApiResponse<object>.Fail("无权访问此订单");
            }

            var reviewedOrderIds = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.OrderId)
                .ToListAsync();

            var statusTextMap = new Dictionary<string, string>
            {
                { "pending", "待支付" },
                { "paid", "已支付" },
                { "confirmed", "已确认" },
                { "staying", "入住中" },
                { "completed", "已完成" },
                { "cancelled", "已取消" },
                { "refunded", "已退款" }
            };

            var result = new OrderItemDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                Status = order.Status,
                StatusText = statusTextMap.GetValueOrDefault(order.Status, order.Status),
                PropertyId = order.PropertyId,
                PropertyTitle = order.Property?.Title ?? "",
                PropertyCoverImage = order.Property?.Images != null && order.Property.Images.Any()
                    ? (order.Property.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? order.Property.Images.First().ImageUrl)
                    : null,
                PropertyAddress = order.Property?.PropertyAddress?.FullAddress ?? "",
                HostName = order.Host?.Name ?? "",
                HostAvatar = order.Host?.Avatar,
                GuestName = order.GuestName,
                GuestPhone = order.GuestPhone,
                CheckInDate = order.CheckInDate,
                CheckOutDate = order.CheckOutDate,
                Guests = order.GuestCount,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt,
                PayDeadline = order.PayDeadline,
                PaidAt = order.PaymentTime,
                PaymentMethod = order.PaymentMethod,
                CancelReason = order.CancelReason,
                HasReviewed = reviewedOrderIds.Contains(order.Id),
                Nights = order.Nights,
                RoomId = order.RoomId,
                RoomName = order.Room?.Name
            };

            return ApiResponse<object>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取订单详情失败, orderId: {OrderId}", orderId);
            return ApiResponse<object>.Fail("获取订单详情失败: " + ex.Message);
        }
    }

    [HttpGet("orders")]
    public async Task<ApiResponse<object>> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] string? orderNo = null,
        [FromQuery] string? idCard = null,
        [FromQuery] string? guestInfo = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<object>.Fail("用户未登录");
        }

        _logger.LogInformation("获取订单列表请求 - UserId: {UserId}, Role: {Role}", userId, GetCurrentRole());

        try
        {
            var role = GetCurrentRole();
            var isAdmin = role == "admin" || role == "super_admin";

            IQueryable<Order> query;

            if (isAdmin)
            {
                query = _context.Orders
                    .Include(o => o.Property)
                    .ThenInclude(p => p.Images)
                    .Include(o => o.Property)
                    .ThenInclude(p => p.PropertyAddress)
                    .Include(o => o.Host)
                    .Include(o => o.User)
                    .Include(o => o.Room)
                    .OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                query = _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Property)
                    .ThenInclude(p => p.Images)
                    .Include(o => o.Property)
                    .ThenInclude(p => p.PropertyAddress)
                    .Include(o => o.Host)
                    .Include(o => o.User)
                    .Include(o => o.Room)
                    .OrderByDescending(o => o.CreatedAt);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = new[] { "pending", "paid", "confirmed", "staying", "completed", "cancelled", "refunded" };
                var statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToLower().Trim())
                    .Where(s => validStatuses.Contains(s))
                    .ToList();

                if (statusList.Count == 0)
                {
                    return ApiResponse<object>.Fail($"无效的状态值: {status}");
                }

                query = query.Where(o => statusList.Contains(o.Status.ToLower()));
            }

            if (!string.IsNullOrEmpty(orderNo))
            {
                query = query.Where(o => o.OrderNo != null && o.OrderNo.Contains(orderNo.Trim()));
            }

            if (!string.IsNullOrEmpty(idCard))
            {
                query = query.Where(o => o.GuestIdCard != null && o.GuestIdCard.Contains(idCard.Trim()));
            }

            if (!string.IsNullOrEmpty(guestInfo))
            {
                var keyword = guestInfo.Trim();
                query = query.Where(o =>
                    (o.GuestName != null && o.GuestName.Contains(keyword)) ||
                    (o.GuestPhone != null && o.GuestPhone.Contains(keyword))
                );
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                if (DateTime.TryParse(startDate, out var start))
                {
                    query = query.Where(o => o.CheckInDate >= start);
                }
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(endDate, out var end))
                {
                    query = query.Where(o => o.CheckInDate <= end);
                }
            }

            var total = await query.CountAsync();
            _logger.LogInformation("订单列表查询 - 总数: {Total}, 页码: {Page}, 每页: {PageSize}", total, page, pageSize);

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderIds = orders.Select(o => o.Id).ToList();
            var reviewedOrderIds = await _context.Reviews
                .Where(r => orderIds.Contains(r.OrderId))
                .Select(r => r.OrderId)
                .ToListAsync();

            var statusTextMap = new Dictionary<string, string>
            {
                { "pending", "待支付" },
                { "paid", "待入住" },
                { "confirmed", "已确认" },
                { "staying", "入住中" },
                { "completed", "已完成" },
                { "cancelled", "已取消" },
                { "refunded", "已退款" }
            };

            var result = orders.Select(o => new OrderItemDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                Status = o.Status,
                StatusText = statusTextMap.GetValueOrDefault(o.Status, o.Status),
                PropertyId = o.PropertyId,
                PropertyTitle = o.Property?.Title ?? "",
                PropertyCoverImage = o.Property?.Images != null && o.Property.Images.Any()
                    ? (o.Property.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? o.Property.Images.First().ImageUrl)
                    : null,
                PropertyAddress = o.Property?.PropertyAddress?.FullAddress ?? "",
                HostName = o.Host?.Name ?? "",
                HostAvatar = o.Host?.Avatar,
                GuestName = o.GuestName,
                GuestPhone = o.GuestPhone,
                CheckInDate = o.CheckInDate,
                CheckOutDate = o.CheckOutDate,
                Guests = o.GuestCount,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                PayDeadline = o.PayDeadline,
                PaidAt = o.PaymentTime,
                PaymentMethod = o.PaymentMethod,
                CancelReason = o.CancelReason,
                HasReviewed = reviewedOrderIds.Contains(o.Id),
                RoomId = o.RoomId,
                RoomName = o.Room != null ? o.Room.Name : null
            }).ToList();

            return ApiResponse<object>.Ok(new
            {
                list = result,
                total = total,
                page = page,
                pageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取订单列表失败");
            return ApiResponse<object>.Fail("获取订单列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="request">取消原因</param>
    /// <returns>操作结果</returns>
    [HttpPost("orders/{orderId}/cancel")]
    public async Task<ApiResponse<bool>> CancelOrder(long orderId, [FromBody] CancelOrderRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var order = await _context.Orders
                .Include(o => o.Property)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId.Value);

            if (order == null)
            {
                return ApiResponse<bool>.Fail("订单不存在");
            }

            if (order.Status == "cancelled" || order.Status == "refunded")
            {
                return ApiResponse<bool>.Fail("订单已取消");
            }

            if (order.Status == "completed")
            {
                return ApiResponse<bool>.Fail("已完成订单无法取消");
            }

            if (order.Status == "staying")
            {
                return ApiResponse<bool>.Fail("入住中的订单无法取消");
            }

            order.Status = "cancelled";
            order.CancelReason = request?.Reason ?? "用户主动取消";
            order.CancelTime = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            if (order.CancelReason.Contains("超时"))
            {
                try
                {
                    await SseController.NotifyUserAsync(
                        order.UserId,
                        "order_timeout",
                        new
                        {
                            orderId = order.Id,
                            orderNo = order.OrderNo,
                            propertyTitle = order.Property?.Title ?? "房源",
                            cancelReason = order.CancelReason,
                            cancelledAt = order.CancelTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                            message = $"您的订单 {order.OrderNo} 已因支付超时被系统自动取消"
                        });
                    _logger.LogInformation("CancelOrder 超时 SSE 推送成功: OrderNo={OrderNo}, UserId={UserId}", order.OrderNo, order.UserId);
                }
                catch (Exception sseEx)
                {
                    _logger.LogWarning(sseEx, "CancelOrder 超时 SSE 推送失败(仍会取消): OrderId={OrderId}", orderId);
                }
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 取消了订单 {OrderId}, 原因: {Reason}", userId, orderId, order.CancelReason);

            return ApiResponse<bool>.Ok(true, "订单已成功取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消订单失败, OrderId: {OrderId}", orderId);
            return ApiResponse<bool>.Fail("取消订单失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 办理入住（房东操作）
    /// </summary>
    [HttpPost("orders/{orderId}/checkin")]
    public async Task<ApiResponse<bool>> CheckIn(long orderId, [FromBody] CheckInRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var order = await _context.Orders
                .Include(o => o.Property)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return ApiResponse<bool>.Fail("订单不存在");
            }

            var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
            if (host == null || order.HostId != host.Id)
            {
                return ApiResponse<bool>.Fail("无权操作此订单");
            }

            if (order.Status == "staying")
            {
                return ApiResponse<bool>.Fail("该订单已办理入住");
            }

            if (order.Status != "confirmed" && order.Status != "paid")
            {
                return ApiResponse<bool>.Fail($"当前订单状态为 {order.Status}，无法办理入住");
            }

            // 自动分配房间：如果订单还没有关联房间
            if (order.RoomId == null)
            {
                // 查找该房源下所有房间
                var propertyRooms = await _context.Rooms
                    .Where(r => r.PropertyId == order.PropertyId)
                    .ToListAsync();

                if (propertyRooms.Any())
                {
                    // 查找在入住日期范围内已被占用的房间
                    var occupiedRoomIds = await _context.Orders
                        .Where(o => o.PropertyId == order.PropertyId
                            && o.RoomId != null
                            && o.Id != orderId
                            && o.Status != "cancelled"
                            && o.Status != "refunded"
                            && o.Status != "completed"
                            && o.CheckInDate < order.CheckOutDate
                            && o.CheckOutDate > order.CheckInDate)
                        .Select(o => o.RoomId.Value)
                        .Distinct()
                        .ToListAsync();

                    // 优先分配空闲房间（Status=1 且未被占用）
                    var availableRoom = propertyRooms
                        .FirstOrDefault(r => r.Status == 1 && !occupiedRoomIds.Contains(r.Id));

                    if (availableRoom != null)
                    {
                        order.RoomId = availableRoom.Id;
                    }
                    else
                    {
                        // 没有空闲房间，尝试分配非维护状态的房间
                        var nonMaintenanceRoom = propertyRooms
                            .FirstOrDefault(r => r.Status != 3 && !occupiedRoomIds.Contains(r.Id));

                        if (nonMaintenanceRoom != null)
                        {
                            order.RoomId = nonMaintenanceRoom.Id;
                        }
                        else
                        {
                            return ApiResponse<bool>.Fail("该房源房间已满，暂无可用房间，请先添加房间或调整已有订单");
                        }
                    }
                }
                else
                {
                    return ApiResponse<bool>.Fail("该房源暂无房间，请先在房间管理中添加房间");
                }
            }

            order.Status = "staying";
            order.GuestIdCard = request.IdCard;
            order.GuestCount = request.Guests;
            order.UpdatedAt = DateTime.Now;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            var roomInfo = order.RoomId.HasValue ? $"，分配房间：{(await _context.Rooms.FindAsync(order.RoomId.Value))?.Name ?? "未知"}" : "";

            _logger.LogInformation(
                "房东 {HostId} 为订单 {OrderId} 办理入住，住客人数：{GuestCount}，身份证：{IdCard}{RoomInfo}",
                userId, orderId, request.Guests, request.IdCard ?? "未提供", roomInfo);

            return ApiResponse<bool>.Ok(true, "入住办理成功" + roomInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "办理入住失败, OrderId: {OrderId}", orderId);
            return ApiResponse<bool>.Fail("办理入住失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 办理退房（房东操作）
    /// </summary>
    [HttpPost("orders/{orderId}/checkout")]
    public async Task<ApiResponse<bool>> CheckOut(long orderId, [FromBody] CheckOutRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return ApiResponse<bool>.Fail("订单不存在");
            }

            var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
            if (host == null || order.HostId != host.Id)
            {
                return ApiResponse<bool>.Fail("无权操作此订单");
            }

            if (order.Status == "completed")
            {
                return ApiResponse<bool>.Fail("该订单已办理退房");
            }

            if (order.Status != "staying")
            {
                return ApiResponse<bool>.Fail($"当前订单状态为 {order.Status}，无法办理退房");
            }

            order.Status = "completed";
            order.PaymentMethod = request.PayMethod;
            order.UpdatedAt = DateTime.Now;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "房东 {HostId} 为订单 {OrderId} 办理退房，支付方式：{PayMethod}",
                userId, orderId, request.PayMethod ?? "未指定");

            return ApiResponse<bool>.Ok(true, "退房办理成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "办理退房失败, OrderId: {OrderId}", orderId);
            return ApiResponse<bool>.Fail("办理退房失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 更新订单状态（通用接口）
    /// </summary>
    [HttpPut("orders/{orderId}/status")]
    public async Task<ApiResponse<bool>> UpdateOrderStatus(long orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return ApiResponse<bool>.Fail("订单不存在");
            }

            var role = GetCurrentRole();
            var isAdmin = role == "admin" || role == "super_admin";

            if (!isAdmin)
            {
                var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
                if (host == null || order.HostId != host.Id)
                {
                    return ApiResponse<bool>.Fail("无权操作此订单");
                }
            }

            var validStatuses = new[] { "pending", "paid", "confirmed", "staying", "completed", "cancelled", "refunded" };
            if (!validStatuses.Contains(request.Status))
            {
                return ApiResponse<bool>.Fail($"无效的状态值: {request.Status}");
            }

            var oldStatus = order.Status;
            order.Status = request.Status;
            order.UpdatedAt = DateTime.Now;

            if (request.GuestCount.HasValue)
            {
                order.GuestCount = request.GuestCount.Value;
            }
            if (!string.IsNullOrEmpty(request.IdCard))
            {
                order.GuestIdCard = request.IdCard;
            }
            if (!string.IsNullOrEmpty(request.PayMethod))
            {
                order.PaymentMethod = request.PayMethod;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "用户 {UserId} 将订单 {OrderId} 状态从 {OldStatus} 更新为 {NewStatus}",
                userId, orderId, oldStatus, request.Status);

            return ApiResponse<bool>.Ok(true, $"订单状态已更新为: {request.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新订单状态失败, OrderId: {OrderId}", orderId);
            return ApiResponse<bool>.Fail("更新订单状态失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取评价列表（管理员可查看所有评价）
    /// </summary>
    [HttpGet("reviews")]
    public async Task<ApiResponse<List<ReviewItemDto>>> GetReviews()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<List<ReviewItemDto>>.Fail("用户未登录");
        }

        try
        {
            var role = GetCurrentRole();
            var isAdmin = role == "admin" || role == "super_admin";

            IQueryable<Models.Review> query;

            if (isAdmin)
            {
                query = _context.Reviews
                    .Include(r => r.Property)
                        .ThenInclude(p => p.Images)
                    .Include(r => r.User)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.CreatedAt);
            }
            else
            {
                query = _context.Reviews
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Property)
                        .ThenInclude(p => p.Images)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.CreatedAt);
            }

            var reviews = await query.ToListAsync();

            var reviewIds = reviews.Select(r => r.Id).ToList();
            var allReplies = await _context.ReviewReplies
                .Where(reply => reviewIds.Contains(reply.ReviewId))
                .OrderByDescending(reply => reply.CreatedAt)
                .ToListAsync();

            var result = reviews.Select(r =>
            {
                string coverImage = null;
                if (r.Property?.Images != null && r.Property.Images.Any())
                {
                    coverImage = r.Property.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl
                        ?? r.Property.Images.First().ImageUrl;
                }

                List<string> reviewImageUrls = new();
                if (r.Images != null && r.Images.Any())
                {
                    reviewImageUrls = r.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList();
                }

                var replies = allReplies
                    .Where(reply => reply.ReviewId == r.Id)
                    .OrderByDescending(reply => reply.CreatedAt)
                    .Select(reply => new ReviewReplyDto
                    {
                        Id = reply.Id,
                        Content = reply.Content,
                        CreatedAt = reply.CreatedAt
                    })
                    .ToList();

                return new ReviewItemDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    PropertyId = r.PropertyId,
                    PropertyTitle = r.Property?.Title ?? "",
                    PropertyCoverImage = coverImage,
                    PropertyAddress = r.Property?.PropertyAddress?.FullAddress ?? "",
                    Rating = (double)r.Rating,
                    CleanlinessRating = r.CleanlinessRating,
                    CommunicationRating = r.CommunicationRating,
                    CheckinRating = r.CheckinRating,
                    AccuracyRating = r.AccuracyRating,
                    LocationRating = r.LocationRating,
                    ValueRating = r.ValueRating,
                    Content = r.Content,
                    IsAnonymous = r.IsAnonymous,
                    CreatedAt = r.CreatedAt,
                    HostReply = r.HostReply,
                    HostReplyTime = r.HostReplyTime,
                    Replies = replies,
                    GuestName = isAdmin ? (r.User?.Nickname ?? r.User?.Username ?? "匿名用户") : null,
                    UserId = r.UserId,
                    Images = reviewImageUrls
                };
            }).ToList();

            return ApiResponse<List<ReviewItemDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评价列表失败");
            return ApiResponse<List<ReviewItemDto>>.Fail("获取评价列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 删除评价（管理员可删除任意评价）
    /// </summary>
    [HttpDelete("reviews/{reviewId}")]
    public async Task<ApiResponse<bool>> DeleteReview(long reviewId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return ApiResponse<bool>.Fail("评价不存在");
            }

            var role = GetCurrentRole();
            var isAdmin = role == "admin" || role == "super_admin";

            if (!isAdmin && review.UserId != userId)
            {
                return ApiResponse<bool>.Fail("无权删除此评价");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 删除了评价 {ReviewId}", userId, reviewId);
            return ApiResponse<bool>.Ok(true, "删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评价失败");
            return ApiResponse<bool>.Fail("删除评价失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 回复评价（支持房东和用户多次回复）
    /// </summary>
    [HttpPost("reviews/{reviewId}/reply")]
    public async Task<ApiResponse<object>> ReplyToReview(long reviewId, [FromBody] ReplyToReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<object>.Fail("用户未登录");
        }

        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return ApiResponse<object>.Fail("评价不存在");
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return ApiResponse<object>.Fail("回复内容不能为空");
            }

            var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value && h.Status == 1);
            var currentUser = await _context.Users.FindAsync(userId.Value);

            var reply = new Models.ReviewReply
            {
                ReviewId = reviewId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            if (host != null)
            {
                reply.HostId = host.Id;
                review.HostReply = request.Content.Trim();
                review.HostReplyTime = DateTime.UtcNow;
            }
            else
            {
                reply.UserId = userId.Value;
            }

            review.UpdatedAt = DateTime.UtcNow;

            _context.ReviewReplies.Add(reply);
            await _context.SaveChangesAsync();

            var replierName = host?.Name ?? currentUser?.Nickname ?? currentUser?.Username ?? "用户";
            _logger.LogInformation("用户 {UserId}({Name}) 回复了评价 {ReviewId}", userId, replierName, reviewId);

            NotifyReviewReplyAsync(review, currentUser, replierName, request.Content.Trim());

            return ApiResponse<object>.Ok(new
            {
                replyId = reply.Id,
                content = reply.Content,
                createdAt = reply.CreatedAt
            }, "回复成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回复评价失败");
            return ApiResponse<object>.Fail("回复评价失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 删除回复
    /// </summary>
    [HttpDelete("reviews/replies/{replyId}")]
    public async Task<ApiResponse<bool>> DeleteReply(long replyId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        try
        {
            var reply = await _context.ReviewReplies.FindAsync(replyId);
            if (reply == null)
            {
                return ApiResponse<bool>.Fail("回复不存在");
            }

            if (reply.HostId.HasValue)
            {
                var host = await _context.Hosts.FirstOrDefaultAsync(h => h.Id == reply.HostId.Value);
                if (host?.UserId != userId.Value)
                {
                    return ApiResponse<bool>.Fail("无权删除此回复");
                }
            }
            else if (reply.UserId.HasValue && reply.UserId.Value != userId.Value)
            {
                return ApiResponse<bool>.Fail("无权删除此回复");
            }

            _context.ReviewReplies.Remove(reply);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 删除了回复 {ReplyId}", userId, replyId);
            return ApiResponse<bool>.Ok(true, "删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除回复失败");
            return ApiResponse<bool>.Fail("删除回复失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 提交评价（支持图片）
    /// </summary>
    [HttpPost("reviews")]
    public async Task<ApiResponse<ReviewItemDto>> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<ReviewItemDto>.Fail("用户未登录");
        }

        try
        {
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
            {
                return ApiResponse<ReviewItemDto>.Fail("订单不存在");
            }

            if (order.UserId != userId)
            {
                var role = GetCurrentRole();
                var isAdmin = role == "admin" || role == "super_admin";
                if (!isAdmin)
                {
                    return ApiResponse<ReviewItemDto>.Fail("无权评价此订单");
                }
            }

            if (order.Status != "completed" && order.Status != "staying")
            {
                return ApiResponse<ReviewItemDto>.Fail("只有已完成或入住中的订单才能评价");
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderId == request.OrderId && r.UserId == order.UserId);

            if (existingReview != null)
            {
                return ApiResponse<ReviewItemDto>.Fail("该订单已评价，不能重复评价");
            }

            var review = new Models.Review
            {
                OrderId = request.OrderId,
                UserId = order.UserId,
                PropertyId = order.PropertyId,
                HostId = order.HostId,
                Rating = (byte)request.Rating,
                CleanlinessRating = request.CleanlinessRating,
                CommunicationRating = request.CommunicationRating,
                CheckinRating = request.CheckinRating,
                AccuracyRating = request.AccuracyRating,
                LocationRating = request.LocationRating,
                ValueRating = request.ValueRating,
                Content = request.Content,
                IsAnonymous = request.IsAnonymous ?? false,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            if (request.Images != null && request.Images.Any())
            {
                int sortOrder = 0;
                foreach (var imageUrl in request.Images)
                {
                    _context.ReviewImages.Add(new Models.ReviewImage
                    {
                        ReviewId = review.Id,
                        ImageUrl = imageUrl,
                        SortOrder = sortOrder++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            var savedReview = await _context.Reviews
                .Include(r => r.Property).ThenInclude(p => p.Images)
                .Include(r => r.Images)
                .FirstAsync(r => r.Id == review.Id);

            List<string> reviewImageUrls = new();
            if (savedReview.Images != null && savedReview.Images.Any())
            {
                reviewImageUrls = savedReview.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList();
            }

            string coverImage = null;
            if (savedReview.Property?.Images != null && savedReview.Property.Images.Any())
            {
                coverImage = savedReview.Property.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl
                    ?? savedReview.Property.Images.First().ImageUrl;
            }

            var result = new ReviewItemDto
            {
                Id = savedReview.Id,
                PropertyId = savedReview.PropertyId,
                PropertyTitle = savedReview.Property?.Title ?? "",
                PropertyCoverImage = coverImage,
                PropertyAddress = savedReview.Property?.PropertyAddress?.FullAddress ?? "",
                Rating = (double)savedReview.Rating,
                Content = savedReview.Content,
                CreatedAt = savedReview.CreatedAt,
                Images = reviewImageUrls
            };

            _logger.LogInformation("用户 {UserId} 提交了评价 {ReviewId}", userId, review.Id);
            return ApiResponse<ReviewItemDto>.Ok(result, "评价提交成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提交评价失败");
            return ApiResponse<ReviewItemDto>.Fail("提交评价失败: " + ex.Message);
        }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst("sub")?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("无法从JWT Token中获取用户ID，Claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string GetCurrentRole()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return roleClaim ?? "user";
    }

    private async void NotifyReviewReplyAsync(Review review, User replier, string replierName, string content)
    {
        try
        {
            if (review.UserId == replier.Id) return;

            var propertyTitle = await _context.Properties
                .Where(p => p.Id == review.PropertyId)
                .Select(p => p.Title)
                .FirstOrDefaultAsync() ?? "房源";

            var title = $"{replierName} 回复了你的评价";
            var message = $"你在「{propertyTitle}」的评价收到了 {replierName} 的回复：\"{TruncateContent(content, 50)}\"";

            await _notificationService.NotifyUserAsync(review.UserId, title, message, "booking");

            await SseController.NotifyUserAsync(
                review.UserId,
                "review_reply",
                new
                {
                    reviewId = review.Id,
                    propertyId = review.PropertyId,
                    propertyTitle = propertyTitle,
                    replierName = replierName,
                    replierAvatar = replier.Avatar,
                    content = content,
                    message = message
                });

            _logger.LogInformation("发送评价回复通知：作者UserId={AuthorId}, 回复者={ReplierName}", review.UserId, replierName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送评价回复通知失败");
        }
    }

    private static string TruncateContent(string content, int maxLen)
    {
        return content.Length <= maxLen ? content : content.Substring(0, maxLen) + "...";
    }

    public class CreateReviewRequest
    {
        public long OrderId { get; set; }
        public int Rating { get; set; }
        public decimal? CleanlinessRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public decimal? CheckinRating { get; set; }
        public decimal? AccuracyRating { get; set; }
        public decimal? LocationRating { get; set; }
        public decimal? ValueRating { get; set; }
        public string? Content { get; set; }
        public bool? IsAnonymous { get; set; }
        public List<string>? Images { get; set; }
    }

    /// <summary>
    /// 回复评价请求
    /// </summary>
    public class ReplyToReviewRequest
    {
        public string Content { get; set; } = "";
    }

    /// <summary>
    /// 办理入住请求
    /// </summary>
    public class CheckInRequest
    {
        public int Guests { get; set; } = 1;
        public string? IdCard { get; set; }
        public decimal? Deposit { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 办理退房请求
    /// </summary>
    public class CheckOutRequest
    {
        public string? PayMethod { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 更新订单状态请求
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = "";
        public int? GuestCount { get; set; }
        public string? IdCard { get; set; }
        public string? PayMethod { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 获取房东控制台数据（今日订单、消息通知、待办事项）
    /// </summary>
    [HttpGet("dashboard/overview")]
    public async Task<ApiResponse<object>> GetDashboardOverview()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<object>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null || host.Status != 1)
        {
            return ApiResponse<object>.Fail("您还不是房东");
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var hostId = host.Id;

        var todayOrdersQuery = _context.Orders
            .Include(o => o.Property)
            .Where(o => o.HostId == hostId &&
                (o.Status == "paid" || o.Status == "staying") &&
                (o.CheckInDate.Date == today || o.CheckOutDate.Date == today))
            .OrderByDescending(o => o.CreatedAt)
            .Take(10);

        var todayOrders = await todayOrdersQuery.Select(o => new
        {
            id = o.Id,
            orderNo = o.OrderNo,
            propertyName = o.Property != null ? o.Property.Title : "未知房源",
            guestName = o.GuestName ?? "匿名用户",
            status = o.Status,
            statusText = o.Status == "paid" ? "待入住" : o.Status == "staying" ? "入住中" : o.Status,
            checkInDate = o.CheckInDate,
            checkOutDate = o.CheckOutDate,
            totalPrice = o.TotalPrice
        }).ToListAsync();

        var messages = new List<object>();

        var newOrders = await _context.Orders
            .Include(o => o.Property)
            .Where(o => o.HostId == hostId && o.Status == "paid" && o.CreatedAt.Date == today)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new { OrderNo = o.OrderNo, GuestName = o.GuestName ?? "匿名用户", PropertyTitle = o.Property != null ? o.Property.Title : "未知房源", CreatedAt = o.CreatedAt })
            .ToListAsync();

        foreach (var order in newOrders)
        {
            messages.Add(new
            {
                id = Guid.NewGuid().ToString(),
                title = "新订单提醒",
                content = $"{order.GuestName}预订了{order.PropertyTitle}",
                time = GetTimeAging(order.CreatedAt),
                type = "order",
                icon = "FileAddOutlined",
                color = "#e8943a",
                bgColor = "rgba(232, 148, 58, 0.1)",
                isRead = false
            });
        }

        var checkinToday = await _context.Orders
            .Where(o => o.HostId == hostId && o.Status == "paid" && o.CheckInDate.Date == today)
            .OrderBy(o => o.CheckInDate)
            .Select(o => new { GuestName = o.GuestName ?? "匿名用户", CheckInDate = o.CheckInDate })
            .ToListAsync();

        foreach (var order in checkinToday)
        {
            messages.Add(new
            {
                id = Guid.NewGuid().ToString(),
                title = "入住提醒",
                content = $"{order.GuestName}将于今日{order.CheckInDate:HH:mm}入住",
                time = GetTimeAging(DateTime.Now.AddHours(-1)),
                type = "checkin",
                icon = "UserAddOutlined",
                color = "#52c41a",
                bgColor = "rgba(82, 196, 26, 0.1)",
                isRead = false
            });
        }

        var checkoutToday = await _context.Orders
            .Where(o => o.HostId == hostId && o.Status == "staying" && o.CheckOutDate.Date == today)
            .OrderBy(o => o.CheckOutDate)
            .Select(o => new { GuestName = o.GuestName ?? "匿名用户", CheckOutDate = o.CheckOutDate })
            .ToListAsync();

        foreach (var order in checkoutToday)
        {
            messages.Add(new
            {
                id = Guid.NewGuid().ToString(),
                title = "退房提醒",
                content = $"{order.GuestName}将于今日{order.CheckOutDate:HH:mm}退房",
                time = GetTimeAging(DateTime.Now.AddHours(-2)),
                type = "checkout",
                icon = "ExportOutlined",
                color = "#faad14",
                bgColor = "rgba(250, 173, 20, 0.1)",
                isRead = false
            });
        }

        var todos = new List<object>();

        foreach (var order in checkinToday)
        {
            todos.Add(new
            {
                id = $"checkin-{Guid.NewGuid()}",
                content = $"准备{order.GuestName}的入住手续",
                priority = "high",
                type = "prepare-checkin"
            });
        }

        foreach (var order in checkoutToday)
        {
            todos.Add(new
            {
                id = $"checkout-{Guid.NewGuid()}",
                content = $"检查{order.GuestName}房间退房状态",
                priority = "normal",
                type = "prepare-checkout"
            });
        }

        var pendingReviews = await _context.Reviews
            .Include(r => r.Order)
            .ThenInclude(o => o.Property)
            .Where(r => r.OrderId > 0 && r.HostReply == null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .Select(r => new { Id = r.Id, Content = r.Content })
            .ToListAsync();

        foreach (var review in pendingReviews)
        {
            var content = string.IsNullOrEmpty(review.Content) ? "新评价" :
                         review.Content.Length > 20 ? review.Content.Substring(0, 20) + "..." :
                         review.Content;
            todos.Add(new
            {
                id = $"review-{review.Id}",
                content = $"回复评价：{content}",
                priority = "normal",
                type = "reply-review",
                reviewId = review.Id
            });
        }

        return ApiResponse<object>.Ok(new
        {
            todayOrders = todayOrders,
            messages = messages.OrderByDescending(m => ((dynamic)m).type == "order" ? 0 : 1).ToList(),
            todos = todos
        });
    }

    private string GetTimeAging(DateTime dateTime)
    {
        var span = DateTime.Now - dateTime;
        if (span.TotalMinutes < 1) return "刚刚";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}分钟前";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}小时前";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}天前";
        return dateTime.ToString("MM-dd");
    }
}

/// <summary>
/// 用户个人资料 DTO
/// </summary>
public class UserProfileDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? IdCard { get; set; }
    public bool IsVerified { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// 更新用户信息请求
/// </summary>
public class UpdateUserProfileRequest
{
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// 实名认证请求
/// </summary>
public class VerifyIdCardRequest
{
    [Required(ErrorMessage = "姓名不能为空")]
    public string RealName { get; set; } = string.Empty;

    [Required(ErrorMessage = "身份证号不能为空")]
    [MaxLength(18, ErrorMessage = "身份证号最长18位")]
    public string IdCard { get; set; } = string.Empty;
}

/// <summary>
/// 更新用户头像请求
/// </summary>
public class UpdateUserAvatarRequest
{
    public string? Avatar { get; set; }
}

/// <summary>
/// 修改密码请求
/// </summary>
public class UpdatePasswordRequest : IValidatableObject
{
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "新密码至少6个字符")]
    public string NewPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OldPassword == NewPassword)
        {
            yield return new ValidationResult("新密码不能与原密码相同", new[] { nameof(NewPassword) });
        }
    }
}

/// <summary>
/// 绑定手机号请求
/// </summary>
public class BindPhoneRequest
{
    [Required(ErrorMessage = "手机号不能为空")]
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证码不能为空")]
    public string VerifyCode { get; set; } = string.Empty;
}

/// <summary>
/// 收藏房源请求
/// </summary>
public class AddFavoriteRequest
{
    [Required(ErrorMessage = "房源ID不能为空")]
    public long PropertyId { get; set; }
}

/// <summary>
/// 收藏列表项DTO
/// </summary>
public class FavoriteItemDto
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 评价列表项DTO
/// </summary>
public class ReviewItemDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCoverImage { get; set; }
    public string PropertyAddress { get; set; } = string.Empty;
    public double Rating { get; set; }
    public decimal? CleanlinessRating { get; set; }
    public decimal? CommunicationRating { get; set; }
    public decimal? CheckinRating { get; set; }
    public decimal? AccuracyRating { get; set; }
    public decimal? LocationRating { get; set; }
    public decimal? ValueRating { get; set; }
    public string? Content { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? HostReply { get; set; }
    public DateTime? HostReplyTime { get; set; }
    public List<ReviewReplyDto> Replies { get; set; } = new();
    public string? GuestName { get; set; }
    public long? UserId { get; set; }
    public List<string> Images { get; set; } = new();
}

public class ReviewReplyDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 订单列表项DTO
/// </summary>
public class OrderItemDto
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public long PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string? PropertyCoverImage { get; set; }
    public string PropertyAddress { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? HostAvatar { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Guests { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PayDeadline { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CancelReason { get; set; }
    public bool HasReviewed { get; set; }
    public int Nights { get; set; }
    public long? RoomId { get; set; }
    public string? RoomName { get; set; }
}

/// <summary>
/// 取消订单请求DTO
/// </summary>
public class CancelOrderRequest
{
    /// <summary>
    /// 取消原因
    /// </summary>
    public string? Reason { get; set; }
}
