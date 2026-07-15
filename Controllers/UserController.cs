using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserManageService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserManageService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="request">查询参数，支持状态筛选、关键词搜索和分页</param>
    /// <returns>分页的用户列表</returns>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<UserListDto>>> GetList([FromQuery] UserQueryRequest request)
    {
        return await _userService.GetListAsync(request);
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<UserListDto>> GetById(long id)
    {
        return await _userService.GetByIdAsync(id);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="request">用户创建请求</param>
    /// <returns>创建的用户信息</returns>
    [HttpPost("create")]
    public async Task<ApiResponse<UserListDto>> Create([FromBody] CreateUserRequest request)
    {
        return await _userService.CreateAsync(request);
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="request">用户更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update(long id, [FromBody] UpdateUserRequest request)
    {
        return await _userService.UpdateAsync(id, request);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        return await _userService.DeleteAsync(id);
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="request">重置密码请求</param>
    /// <returns>重置结果</returns>
    [HttpPost("{id}/reset-password")]
    public async Task<ApiResponse<bool>> ResetPassword(long id, [FromBody] ResetUserPasswordRequest request)
    {
        return await _userService.ResetPasswordAsync(id, request.NewPassword);
    }

    /// <summary>
    /// 切换用户启用/禁用状态
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/toggle-status")]
    public async Task<ApiResponse<bool>> ToggleStatus(long id)
    {
        return await _userService.ToggleStatusAsync(id);
    }
}
