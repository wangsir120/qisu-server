using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AlipayService _alipayService;
        private readonly ILogger<PaymentController> _logger;
        private readonly AppDbContext _dbContext;

        public PaymentController(
            AlipayService alipayService,
            ILogger<PaymentController> logger,
            AppDbContext dbContext)
        {
            _alipayService = alipayService;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ApiResponse<string>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var orderId = $"QX{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

                var propertyId = long.TryParse(request.PropertyId, out var pid) ? pid : 1;

                var property = await _dbContext.Properties
                    .Include(p => p.Host)
                    .FirstOrDefaultAsync(p => p.Id == propertyId);

                long hostId;
                if (property?.Host != null)
                {
                    hostId = property.Host.Id;
                }
                else
                {
                    var anyHost = await _dbContext.Hosts.FirstOrDefaultAsync();
                    if (anyHost == null)
                    {
                        return ApiResponse<string>.Fail("系统中暂无房东数据，无法创建订单");
                    }
                    hostId = anyHost.Id;
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return ApiResponse<string>.Fail("用户未登录，请先登录");
                }

                _logger.LogInformation("创建订单 - 用户ID: {UserId}, 房源ID: {PropertyId}", userId.Value, propertyId);

                var order = new Order
                {
                    OrderNo = orderId,
                    UserId = userId.Value,
                    PropertyId = propertyId,
                    HostId = hostId,
                    CheckInDate = DateTime.Parse(request.CheckIn),
                    CheckOutDate = DateTime.Parse(request.CheckOut),
                    Nights = (DateTime.Parse(request.CheckOut) - DateTime.Parse(request.CheckIn)).Days,
                    GuestCount = request.Guests,
                    GuestName = request.GuestName ?? "",
                    GuestPhone = request.GuestPhone ?? "",
                    GuestIdCard = request.GuestIdCard ?? "",
                    PricePerNight = Math.Round(request.TotalAmount * 0.9m / Math.Max(1, (DateTime.Parse(request.CheckOut) - DateTime.Parse(request.CheckIn)).Days), 2),
                    Subtotal = Math.Round(request.TotalAmount * 0.9m, 2),
                    CleaningFee = Math.Round(request.TotalAmount * 0.05m, 2),
                    ServiceFee = Math.Round(request.TotalAmount * 0.05m, 2),
                    TotalPrice = request.TotalAmount,
                    Status = "pending",
                    PaymentMethod = "alipay",
                    PayDeadline = DateTime.Now.AddMinutes(5),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _dbContext.Orders.AddAsync(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("订单已创建: {OrderId}, 状态: pending (待支付), HostId: {HostId}", orderId, hostId);

                var formHtml = _alipayService.PagePay(
                    outTradeNo: orderId,
                    totalAmount: request.TotalAmount.ToString("F2"),
                    subject: request.Subject,
                    body: request.Body
                );

                _logger.LogInformation("支付订单创建成功: {OrderId}", orderId);

                return ApiResponse<string>.Ok(formHtml, "支付订单创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付订单失败");
                return ApiResponse<string>.Fail("创建支付订单失败: " + ex.Message);
            }
        }

        [HttpPost("notify")]
        [AllowAnonymous]
        public async Task<IActionResult> AlipayNotify()
        {
            try
            {
                _logger.LogInformation("收到支付宝异步通知回调");

                var formData = await Request.ReadFormAsync();
                var dict = new Dictionary<string, string>();

                foreach (var key in formData.Keys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        dict[key] = formData[key];
                    }
                }

                _logger.LogInformation("关键参数 - out_trade_no: {OutTradeNo}, trade_status: {TradeStatus}, total_amount: {TotalAmount}",
                    dict.GetValueOrDefault("out_trade_no", "N/A"),
                    dict.GetValueOrDefault("trade_status", "N/A"),
                    dict.GetValueOrDefault("total_amount", "N/A"));

                bool signVerified = _alipayService.VerifySign(dict);

                _logger.LogInformation("签名验证结果: {Result}", signVerified ? "通过" : "失败");

                if (signVerified)
                {
                    var tradeStatus = formData["trade_status"];
                    var outTradeNo = formData["out_trade_no"];
                    var tradeNo = formData["trade_no"];
                    var totalAmount = formData["total_amount"];

                    _logger.LogInformation("支付宝回调验证成功 - 订单号:{OutTradeNo}, 支付宝交易号:{TradeNo}, 状态:{TradeStatus}, 金额:{TotalAmount}",
                        outTradeNo, tradeNo, tradeStatus, totalAmount);

                    if (tradeStatus == "TRADE_SUCCESS" || tradeStatus == "TRADE_FINISHED")
                    {
                        _logger.LogInformation("开始处理支付成功逻辑, 订单号: {OutTradeNo}", outTradeNo);

                        var order = await _dbContext.Orders
                            .FirstOrDefaultAsync(o => o.OrderNo == outTradeNo);

                        if (order != null)
                        {
                            _logger.LogInformation("找到订单 - 当前状态: {Status}, 订单ID: {Id}", order.Status, order.Id);

                            order.Status = "paid";
                            order.PaymentTime = DateTime.Now;
                            order.UpdatedAt = DateTime.Now;

                            _dbContext.Orders.Update(order);
                            await _dbContext.SaveChangesAsync();

                            _logger.LogInformation("订单 {OutTradeNo} 支付完成，金额: {TotalAmount}，状态更新为: paid (待入住)", outTradeNo, totalAmount);
                        }
                        else
                        {
                            _logger.LogWarning("未找到订单: {OutTradeNo}", outTradeNo);
                        }
                    }
                    else if (tradeStatus == "TRADE_CLOSED")
                    {
                        var order = await _dbContext.Orders
                            .FirstOrDefaultAsync(o => o.OrderNo == outTradeNo);

                        if (order != null && order.Status == "pending")
                        {
                            order.Status = "cancelled";
                            order.CancelTime = DateTime.Now;
                            order.CancelReason = "用户未支付，交易超时关闭";
                            order.UpdatedAt = DateTime.Now;

                            _dbContext.Orders.Update(order);
                            await _dbContext.SaveChangesAsync();

                            _logger.LogInformation("订单 {OutTradeNo} 已超时关闭，状态更新为: cancelled", outTradeNo);
                        }
                        else
                        {
                            _logger.LogInformation("订单 {OutTradeNo} 状态为 {Status}，无需处理 TRADE_CLOSED", outTradeNo, order?.Status);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("未处理的交易状态: {TradeStatus}, 订单号: {OutTradeNo}", tradeStatus, outTradeNo);
                    }

                    return Content("success");
                }
                else
                {
                    _logger.LogWarning("支付宝签名验证失败");
                    _logger.LogWarning("失败详情 - 参数列表: {Params}", string.Join(", ", dict.Keys));

                    return Content("failure");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付宝通知异常: {Message}", ex.Message);
                _logger.LogError(ex, "异常堆栈: {StackTrace}", ex.StackTrace);
                return Content("failure");
            }
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> AlipayReturn()
        {
            var outTradeNo = Request.Query["out_trade_no"];
            var tradeNo = Request.Query["trade_no"];
            var totalAmount = Request.Query["total_amount"];

            _logger.LogInformation("支付宝同步返回(备用) - 订单号:{OutTradeNo}, 交易号:{TradeNo}",
                outTradeNo, tradeNo);

            if (!string.IsNullOrEmpty(outTradeNo))
            {
                var order = await _dbContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderNo == outTradeNo);

                if (order != null && order.Status == "pending")
                {
                    _logger.LogInformation("订单 {OrderNo} 状态为pending，主动向支付宝查询确认", outTradeNo);

                    var queryResult = await _alipayService.QueryTradeAsync(outTradeNo);

                    if (queryResult.IsPaid)
                    {
                        order.Status = "paid";
                        order.PaymentTime = DateTime.Now;
                        order.UpdatedAt = DateTime.Now;
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("[异步返回备用] 订单 {OrderNo} 已更新为: paid", outTradeNo);
                    }
                    else if (queryResult.IsClosed)
                    {
                        order.Status = "cancelled";
                        order.CancelTime = DateTime.Now;
                        order.CancelReason = "支付超时或被关闭";
                        order.UpdatedAt = DateTime.Now;
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("[异步返回备用] 订单 {OrderNo} 已更新为: cancelled", outTradeNo);
                    }
                }
            }

            return Redirect($"http://localhost:5754/qixu-web/client/index/booking/success" +
                $"?orderId={outTradeNo}&total={totalAmount}&tradeNo={tradeNo}");
        }

        [HttpGet("query/{orderId}")]
        public async Task<ApiResponse<object>> QueryPayment(string orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Property)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.OrderNo == orderId || o.Id.ToString() == orderId);

            if (order == null)
            {
                return ApiResponse<object>.Fail("订单不存在");
            }

            if (order.Status == "pending")
            {
                _logger.LogInformation("订单 {OrderNo} 状态为pending，主动向支付宝查询确认支付结果", order.OrderNo);

                var queryResult = await _alipayService.QueryTradeAsync(order.OrderNo);

                if (queryResult.IsPaid)
                {
                    order.Status = "paid";
                    order.PaymentTime = DateTime.Now;
                    order.UpdatedAt = DateTime.Now;

                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("[查询确认] 订单 {OrderNo} 支付宝返回已支付，状态更新为: paid", order.OrderNo);
                }
                else if (queryResult.IsClosed)
                {
                    order.Status = "cancelled";
                    order.CancelTime = DateTime.Now;
                    order.CancelReason = "支付超时或被用户关闭";
                    order.UpdatedAt = DateTime.Now;

                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("[查询确认] 订单 {OrderNo} 已被关闭，状态更新为: cancelled", order.OrderNo);
                }
                else if (queryResult.IsNotExist)
                {
                    _logger.LogWarning("[查询确认] 支付宝侧无此订单: {OrderNo}", order.OrderNo);
                }
                else
                {
                    _logger.LogInformation("[查询确认] 支付宝查询结果: Success={Success}, Status={Status}",
                        queryResult.Success, queryResult.TradeStatus);
                }
            }

            return ApiResponse<object>.Ok(new
            {
                OrderId = order.OrderNo,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                PaymentTime = order.PaymentTime,
                CreatedAt = order.CreatedAt,
                PropertyTitle = order.Property?.Title ?? "",
                PropertyImage = order.Property?.Images?.FirstOrDefault(i => i.IsCover)?.ImageUrl
                    ?? order.Property?.Images?.FirstOrDefault()?.ImageUrl
                    ?? "",
                CheckInDate = order.CheckInDate.ToString("yyyy-MM-dd"),
                CheckOutDate = order.CheckOutDate.ToString("yyyy-MM-dd"),
                Nights = order.Nights,
                GuestCount = order.GuestCount
            });
        }

        [HttpPost("repay")]
        public async Task<ApiResponse<string>> RepayPayment([FromBody] RepayRequest request)
        {
            try
            {
                Order? order = null;

                if (long.TryParse(request.OrderId, out long numericId))
                {
                    order = await _dbContext.Orders.FindAsync(numericId);
                }

                if (order == null)
                {
                    order = await _dbContext.Orders
                        .FirstOrDefaultAsync(o => o.OrderNo == request.OrderId);
                }

                if (order == null)
                {
                    return ApiResponse<string>.Fail("订单不存在");
                }

                _logger.LogInformation("重新支付请求 - 订单号: {OrderNo}, 当前状态: {Status}", order.OrderNo, order.Status);

                if (order.Status == "paid")
                {
                    return ApiResponse<string>.Fail("该订单已支付，无需重复支付");
                }

                if (order.Status == "cancelled")
                {
                    return ApiResponse<string>.Fail("该订单已取消，无法支付");
                }

                if (order.Status != "pending")
                {
                    return ApiResponse<string>.Fail($"当前订单状态为 {order.Status}，无法支付");
                }

                if (order.PayDeadline.HasValue && DateTime.Now > order.PayDeadline.Value)
                {
                    order.Status = "cancelled";
                    order.CancelTime = DateTime.Now;
                    order.CancelReason = "支付超时，系统自动取消";
                    order.UpdatedAt = DateTime.Now;

                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogWarning("订单 {OrderNo} 已超时，自动取消", order.OrderNo);
                    return ApiResponse<string>.Fail("支付已超时，订单已自动取消");
                }

                var formHtml = _alipayService.PagePay(
                    outTradeNo: order.OrderNo,
                    totalAmount: order.TotalPrice.ToString("F2"),
                    subject: $"栖宿民宿-{order.Property?.Title ?? "订单支付"}",
                    body: $"订单号: {order.OrderNo}, 入住人: {order.GuestName}"
                );

                _logger.LogInformation("重新支付表单生成成功: {OrderNo}", order.OrderNo);

                return ApiResponse<string>.Ok(formHtml, "重新支付订单创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新支付失败");
                return ApiResponse<string>.Fail("重新支付失败: " + ex.Message);
            }
        }

        public class CreatePaymentRequest
        {
            public decimal TotalAmount { get; set; }
            public string Subject { get; set; } = "";
            public string Body { get; set; } = "";
            public string PropertyId { get; set; } = "";
            public string CheckIn { get; set; } = "";
            public string CheckOut { get; set; } = "";
            public int Guests { get; set; } = 1;
            public string? GuestName { get; set; }
            public string? GuestPhone { get; set; }
            public string? GuestIdCard { get; set; }
        }

        public class RepayRequest
        {
            public string OrderId { get; set; } = "";
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
    }
}
