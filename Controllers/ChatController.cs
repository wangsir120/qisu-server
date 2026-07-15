using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Middleware;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 在线客服聊天控制器
/// </summary>
[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatController> _logger;

    public ChatController(AppDbContext context, ILogger<ChatController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取会话列表
    /// </summary>
    /// <returns>会话列表，管理员可查看所有用户会话，普通用户只能查看自己的会话</returns>
    [HttpGet("conversations")]
    public async Task<ApiResponse<List<ConversationDto>>> GetConversations()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<List<ConversationDto>>.Fail("用户未登录");
            }

            var isAdmin = await IsAdmin(userId.Value);
            _logger.LogInformation("获取会话列表: UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);

            List<ChatConversation> conversations;
            if (isAdmin)
            {
                conversations = await _context.ChatConversations
                    .Include(c => c.User)
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToListAsync();
                
                var groupedConversations = conversations
                    .GroupBy(c => c.UserId)
                    .Select(g => g.First())
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToList();
                conversations = groupedConversations;
            }
            else
            {
                conversations = await _context.ChatConversations
                    .Where(c => c.UserId == userId.Value)
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToListAsync();
            }

            var result = conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                ConversationId = c.ConversationId,
                UserId = c.UserId,
                UserName = c.User?.Nickname ?? c.User?.Username ?? c.User?.Phone ?? "用户" + c.UserId,
                UserAvatar = c.User?.Avatar,
                AdminId = c.AdminId,
                LastMessage = c.LastMessage,
                LastMessageTime = c.LastMessageTime,
                UnreadCount = c.UnreadCount,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList();

            return ApiResponse<List<ConversationDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话列表失败");
            return ApiResponse<List<ConversationDto>>.Fail("获取会话列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取会话消息列表
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认50</param>
    /// <returns>消息列表</returns>
    [HttpGet("messages/{conversationId}")]
    public async Task<ApiResponse<List<ChatMessageDto>>> GetMessages(string conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<List<ChatMessageDto>>.Fail("用户未登录");
        }

        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        
        if (conversation == null)
        {
            return ApiResponse<List<ChatMessageDto>>.Ok(new List<ChatMessageDto>());
        }

        var messages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var userSenders = await _context.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);
        var adminSenders = await _context.Admins
            .Where(a => senderIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a);

        var result = messages.Select(m => {
            var isAdmin = m.SenderId != conversation.UserId;
            string senderName;
            string? senderAvatar = null;
            
            if (isAdmin)
            {
                adminSenders.TryGetValue(m.SenderId, out var admin);
                senderName = admin?.Name ?? admin?.Username ?? "客服";
                senderAvatar = admin?.Avatar;
            }
            else
            {
                userSenders.TryGetValue(m.SenderId, out var sender);
                senderName = sender?.Nickname ?? sender?.Username ?? sender?.Phone ?? "用户";
                senderAvatar = sender?.Avatar;
            }
            
            _logger.LogInformation("消息ID: {MsgId}, SenderId: {SenderId}, UserId: {UserId}, IsAdmin: {IsAdmin}, SenderName: {SenderName}", 
                m.Id, m.SenderId, conversation.UserId, isAdmin, senderName);
            
            return new ChatMessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                SenderName = senderName,
                SenderAvatar = senderAvatar,
                Content = m.Content,
                MessageType = m.MessageType,
                IsRead = m.IsRead,
                IsAdmin = isAdmin,
                CreatedAt = m.CreatedAt
            };
        }).ToList();

        return ApiResponse<List<ChatMessageDto>>.Ok(result);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="request">发送请求，包含会话ID、消息内容、消息类型、接收人ID</param>
    /// <returns>发送的消息信息</returns>
    /// <remarks>
    /// 发送后会通过WebSocket实时推送给对方
    /// </remarks>
    [HttpPost("send")]
    public async Task<ApiResponse<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<ChatMessageDto>.Fail("用户未登录");
        }

        var isAdmin = await IsAdmin(userId.Value);

        string conversationId = request.ConversationId ?? Guid.NewGuid().ToString("N");

        if (string.IsNullOrEmpty(request.ConversationId))
        {
            var existingConversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.Status == "active");

            if (existingConversation != null)
            {
                conversationId = existingConversation.ConversationId;
            }
            else
            {
                var newConversation = new ChatConversation
                {
                    ConversationId = conversationId,
                    UserId = userId.Value,
                    AdminId = 1001,
                    LastMessage = request.Content,
                    LastMessageTime = DateTime.Now,
                    Status = "active"
                };
                _context.ChatConversations.Add(newConversation);
            }
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = userId.Value,
            ReceiverId = isAdmin ? request.ReceiverId : 1001,
            Content = request.Content,
            MessageType = request.MessageType ?? "text",
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.ChatMessages.Add(message);

        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conversation != null)
        {
            conversation.LastMessage = request.Content;
            conversation.LastMessageTime = DateTime.Now;
            if (!isAdmin)
            {
                conversation.UnreadCount++;
            }
        }

        await _context.SaveChangesAsync();

        var sender = await _context.Users.FindAsync(userId.Value);
        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = sender?.Nickname ?? sender?.Username ?? "用户",
            SenderAvatar = sender?.Avatar,
            Content = message.Content,
            MessageType = message.MessageType,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };

        long targetUserId = isAdmin ? request.ReceiverId ?? 1 : 1;
        await WebSocketMiddleware.SendToUserAsync(targetUserId, "chat", new
        {
            id = message.Id,
            conversationId = message.ConversationId,
            senderId = message.SenderId,
            senderName = messageDto.SenderName,
            senderAvatar = messageDto.SenderAvatar,
            content = message.Content,
            messageType = message.MessageType,
            timestamp = message.CreatedAt
        });

        return ApiResponse<ChatMessageDto>.Ok(messageDto);
    }

    /// <summary>
    /// 标记会话消息为已读
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("mark-read/{conversationId}")]
    public async Task<ApiResponse<bool>> MarkAsRead(string conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId && !m.IsRead && m.SenderId != userId.Value)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation != null)
        {
            conversation.UnreadCount = 0;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "已标记为已读");
    }

    /// <summary>
    /// 获取未读消息数量
    /// </summary>
    /// <returns>未读消息总数，管理员返回所有会话的未读数，普通用户返回自己的未读数</returns>
    [HttpGet("unread-count")]
    public async Task<ApiResponse<int>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<int>.Fail("用户未登录");
        }

        var isAdmin = await IsAdmin(userId.Value);

        int unreadCount;
        if (isAdmin)
        {
            unreadCount = await _context.ChatConversations
                .SumAsync(c => c.UnreadCount);
        }
        else
        {
            unreadCount = await _context.ChatMessages
                .Where(m => m.ReceiverId == userId.Value && !m.IsRead)
                .CountAsync();
        }

        return ApiResponse<int>.Ok(unreadCount);
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
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task<bool> IsAdmin(long userId)
    {
        var admin = await _context.Admins.FindAsync(userId);
        return admin != null;
    }
}

public class ConversationDto
{
    public long Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public long? AdminId { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class ChatMessageDto
{
    public long Id { get; set; }
    public string? ConversationId { get; set; }
    public long SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
    public bool IsRead { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageRequest
{
    public string? ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; }
    public long? ReceiverId { get; set; }
}
