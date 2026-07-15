using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IUserService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<LoginResponse> AdminLoginAsync(LoginRequest request);
    Task<LoginResponse> HostLoginAsync(LoginRequest request);
    Task<ApiResponse<bool>> CheckPhoneExistsAsync(string phone);
    Task<ApiResponse<bool>> CheckHostPhoneExistsAsync(string phone);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ISmsService _smsService;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, IJwtService jwtService, ISmsService smsService, ILogger<UserService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Phone == request.Username);

        if (user == null)
        {
            return new LoginResponse { Success = false, Message = "用户不存在" };
        }

        if (request.Password != user.Password)
        {
            return new LoginResponse { Success = false, Message = "密码错误" };
        }

        if (user.Status != 1)
        {
            return new LoginResponse { Success = false, Message = "账号已被禁用" };
        }

        var lastLoginAt = user.LastLoginAt;
        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == user.Id);
        var role = host != null && host.Status == 1 ? "landlord" : "user";

        var token = _jwtService.GenerateToken(user.Id, user.Username, role);

        return new LoginResponse
        {
            Success = true,
            Message = "登录成功",
            User = new UserData
            {
                Id = user.Id,
                Username = user.Username,
                Phone = user.Phone,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt,
                Nickname = user.Nickname ?? user.Username,
                Email = user.Email,
                Gender = user.Gender,
                IdCard = user.IdCard,
                IsVerified = user.IsVerified,
                Role = role,
                LastLoginAt = lastLoginAt
            },
            Token = token
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Phone == request.Phone);

        if (existingUser != null)
        {
            if (existingUser.Username == request.Username)
                return new LoginResponse { Success = false, Message = "用户名已存在" };
            if (existingUser.Phone == request.Phone)
                return new LoginResponse { Success = false, Message = "手机号已被注册" };
        }

        var user = new User
        {
            Username = request.Username,
            Password = request.Password,
            Phone = request.Phone,
            Nickname = request.Username,
            Status = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.Username, "user");

        return new LoginResponse
        {
            Success = true,
            Message = "注册成功",
            User = new UserData
            {
                Id = user.Id,
                Username = user.Username,
                Phone = user.Phone,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Email = user.Email,
                Gender = user.Gender,
                IdCard = user.IdCard,
                IsVerified = user.IsVerified,
                Role = "user",
                CreatedAt = user.CreatedAt
            },
            Token = token
        };
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);

        if (user == null)
        {
            return ApiResponse<bool>.Fail("该手机号未注册");
        }

        user.Password = request.NewPassword;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "密码重置成功");
    }

    public async Task<ApiResponse<bool>> CheckPhoneExistsAsync(string phone)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);

        if (user == null)
        {
            return ApiResponse<bool>.Fail("该手机号未注册");
        }

        return ApiResponse<bool>.Ok(true, "该手机号已注册");
    }

    public async Task<ApiResponse<bool>> CheckHostPhoneExistsAsync(string phone)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);

        if (user == null)
        {
            return ApiResponse<bool>.Fail("该手机号未注册");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == user.Id);

        if (host == null)
        {
            return ApiResponse<bool>.Fail("该账号不是房东");
        }

        return ApiResponse<bool>.Ok(true, "该手机号已注册为房东");
    }

    public async Task<LoginResponse> AdminLoginAsync(LoginRequest request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == request.Username);

        if (admin == null)
        {
            return new LoginResponse { Success = false, Message = "管理员账号不存在" };
        }

        if (request.Password != admin.Password)
        {
            return new LoginResponse { Success = false, Message = "密码错误" };
        }

        if (!admin.Status)
        {
            return new LoginResponse { Success = false, Message = "账号已被禁用" };
        }

        var lastLoginAt = admin.LastLoginAt;
        admin.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(admin.Id, admin.Username, admin.Role ?? "admin");

        return new LoginResponse
        {
            Success = true,
            Message = "登录成功",
            User = new UserData
            {
                Id = admin.Id,
                Username = admin.Username,
                Name = admin.Name,
                Nickname = admin.Name ?? admin.Username,
                Avatar = admin.Avatar,
                Phone = admin.Phone,
                Email = admin.Email,
                Role = admin.Role ?? "admin",
                LastLoginAt = lastLoginAt
            },
            Token = token
        };
    }

    public async Task<LoginResponse> HostLoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Phone == request.Username);

        if (user == null)
        {
            return new LoginResponse { Success = false, Message = "账号不存在" };
        }

        if (request.Password != user.Password)
        {
            return new LoginResponse { Success = false, Message = "密码错误" };
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == user.Id);
        if (host == null)
        {
            return new LoginResponse { Success = false, Message = "您还不是房东，请先申请成为房东" };
        }

        if (user.Status != 1)
        {
            return new LoginResponse { Success = false, Message = "账号已被禁用" };
        }

        if (host.Status != 1)
        {
            return new LoginResponse { Success = false, Message = "房东账号已被禁用" };
        }

        var lastLoginAt = user.LastLoginAt;
        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.Username, "landlord");

        return new LoginResponse
        {
            Success = true,
            Message = "登录成功",
            User = new UserData
            {
                Id = user.Id,
                Username = user.Username,
                Name = host.Name,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Nickname = user.Nickname ?? user.Username,
                Role = "landlord",
                LastLoginAt = lastLoginAt
            },
            Token = token
        };
    }
}
