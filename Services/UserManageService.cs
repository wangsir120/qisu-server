using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IUserManageService
{
    Task<ApiResponse<PagedResult<UserListDto>>> GetListAsync(UserQueryRequest request);
    Task<ApiResponse<UserListDto>> GetByIdAsync(long id);
    Task<ApiResponse<UserListDto>> CreateAsync(CreateUserRequest request);
    Task<ApiResponse<bool>> UpdateAsync(long id, UpdateUserRequest request);
    Task<ApiResponse<bool>> DeleteAsync(long id);
    Task<ApiResponse<bool>> ResetPasswordAsync(long id, string newPassword);
    Task<ApiResponse<bool>> ToggleStatusAsync(long id);
}

public class UserManageService : IUserManageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserManageService> _logger;

    public UserManageService(AppDbContext context, ILogger<UserManageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<UserListDto>>> GetListAsync(UserQueryRequest request)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(u => u.Username.Contains(request.Keyword) || 
                                     (u.Phone != null && u.Phone.Contains(request.Keyword)) ||
                                     (u.Nickname != null && u.Nickname.Contains(request.Keyword)));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusValue = request.Status == "active" ? (byte)1 : (byte)0;
            query = query.Where(u => u.Status == statusValue);
        }

        var total = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Nickname = u.Nickname,
                Phone = u.Phone,
                Avatar = u.Avatar,
                Status = u.Status == (byte)1 ? "active" : "inactive",
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<UserListDto>
        {
            Items = users,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<UserListDto>>.Ok(result);
    }

    public async Task<ApiResponse<UserListDto>> GetByIdAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            return ApiResponse<UserListDto>.Ok(new UserListDto
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Status = user.Status == (byte)1 ? "active" : "inactive",
                CreatedAt = user.CreatedAt
            });
        }

        return ApiResponse<UserListDto>.Fail("用户不存在");
    }

    public async Task<ApiResponse<UserListDto>> CreateAsync(CreateUserRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return ApiResponse<UserListDto>.Fail("用户名已存在");
        }

        var user = new User
        {
            Username = request.Username,
            Password = request.Password,
            Nickname = request.Nickname ?? request.Username,
            Phone = request.Phone,
            Status = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ApiResponse<UserListDto>.Ok(new UserListDto
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Phone = user.Phone,
            Avatar = user.Avatar,
            Status = "active",
            CreatedAt = user.CreatedAt
        }, "创建成功");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(long id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            if (request.Nickname != null) user.Nickname = request.Nickname;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.Status != null) user.Status = request.Status == "active" ? (byte)1 : (byte)0;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "更新成功");
        }

        return ApiResponse<bool>.Fail("用户不存在");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "删除成功");
        }

        return ApiResponse<bool>.Fail("用户不存在");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(long id, string newPassword)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.Password = newPassword;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "密码重置成功");
        }

        return ApiResponse<bool>.Fail("用户不存在");
    }

    public async Task<ApiResponse<bool>> ToggleStatusAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.Status = (byte)(user.Status == (byte)1 ? 0 : 1);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "操作成功");
        }

        return ApiResponse<bool>.Fail("用户不存在");
    }
}
