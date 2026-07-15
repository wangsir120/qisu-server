using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using qisu_server.Config;

namespace qisu_server.Services
{
    public class AlipayService
    {
        private readonly QisuAlipayConfig _config;
        private readonly ILogger<AlipayService> _logger;
        private readonly HttpClient _httpClient;

        public AlipayService(IOptions<QisuAlipayConfig> config, ILogger<AlipayService> logger, HttpClient httpClient)
        {
            _config = config?.Value ?? new QisuAlipayConfig();
            _logger = logger;
            _httpClient = httpClient;
        }

        public string PagePay(string outTradeNo, string totalAmount, string subject, string body = "")
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "app_id", _config.AppId },
                    { "method", "alipay.trade.page.pay" },
                    { "charset", _config.Charset },
                    { "sign_type", _config.SignType },
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "version", _config.Version },
                    { "notify_url", _config.NotifyUrl ?? "" },
                    { "return_url", _config.ReturnUrl ?? "" }
                };

                var bizContent = new Dictionary<string, string>
                {
                    { "out_trade_no", outTradeNo },
                    { "total_amount", totalAmount },
                    { "subject", subject },
                    { "body", body },
                    { "product_code", "FAST_INSTANT_TRADE_PAY" }
                };

                parameters["biz_content"] = System.Text.Json.JsonSerializer.Serialize(bizContent);

                string sign = GenerateSign(parameters);
                parameters["sign"] = sign;

                string formHtml = BuildFormHtml(parameters);

                _logger.LogInformation("创建支付宝订单: {OrderId}, 金额: {Amount}", outTradeNo, totalAmount);

                return formHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付宝订单失败: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<QueryTradeResult> QueryTradeAsync(string outTradeNo)
        {
            try
            {
                _logger.LogInformation("主动查询支付宝订单状态: {OutTradeNo}", outTradeNo);

                var bizContent = new Dictionary<string, string>
                {
                    { "out_trade_no", outTradeNo }
                };

                var parameters = new Dictionary<string, string>
                {
                    { "app_id", _config.AppId },
                    { "method", "alipay.trade.query" },
                    { "charset", _config.Charset },
                    { "sign_type", _config.SignType },
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "version", _config.Version },
                    { "biz_content", JsonSerializer.Serialize(bizContent) }
                };

                string sign = GenerateSign(parameters);
                parameters["sign"] = sign;

                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(_config.GatewayUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("查询结果原始响应: {Response}", responseBody);

                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var queryResponse = result.TryGetProperty("alipay_trade_query_response", out var tradeResp)
                    ? tradeResp
                    : result;

                var code = queryResponse.TryGetProperty("code", out var c) ? c.GetString() : "";
                var subCode = queryResponse.TryGetProperty("sub_code", out var sc) ? sc.GetString() : "";
                var msg = queryResponse.TryGetProperty("msg", out var m) ? m.GetString() : "";

                if (code == "10000")
                {
                    var tradeStatus = queryResponse.TryGetProperty("trade_status", out var ts) ? ts.GetString() : "";
                    var tradeNo = queryResponse.TryGetProperty("trade_no", out var tn) ? tn.GetString() : "";
                    var totalAmount = queryResponse.TryGetProperty("total_amount", out var ta) ? ta.GetString() : "";
                    var buyerLogonId = queryResponse.TryGetProperty("buyer_logon_id", out var bl) ? bl.GetString() : "";

                    _logger.LogInformation("查询成功 - 订单号:{OutTradeNo}, 交易号:{TradeNo}, 状态:{TradeStatus}, 金额:{TotalAmount}",
                        outTradeNo, tradeNo, tradeStatus, totalAmount);

                    return new QueryTradeResult
                    {
                        Success = true,
                        TradeNo = tradeNo ?? "",
                        TradeStatus = tradeStatus ?? "",
                        TotalAmount = totalAmount ?? "",
                        BuyerLogonId = buyerLogonId ?? ""
                    };
                }
                else if (code == "20000" && subCode == "ACQ.TRADE_NOT_EXIST")
                {
                    _logger.LogWarning("支付宝查询 - 订单不存在: {OutTradeNo}", outTradeNo);
                    return new QueryTradeResult { Success = true, TradeStatus = "TRADE_NOT_EXIST" };
                }
                else
                {
                    _logger.LogWarning("支付宝查询失败 - code:{Code}, sub_code:{SubCode}, msg:{Msg}", code, subCode, msg);
                    return new QueryTradeResult { Success = false, ErrorMessage = msg ?? "查询失败" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "支付宝查询异常: {Message}", ex.Message);
                return new QueryTradeResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public bool VerifySign(IDictionary<string, string> paramsDict)
        {
            try
            {
                _logger.LogInformation("开始验证支付宝签名");

                if (paramsDict == null || paramsDict.Count == 0)
                {
                    _logger.LogWarning("签名验证失败: 参数为空");
                    return false;
                }

                if (!paramsDict.TryGetValue("sign", out string? sign) || string.IsNullOrEmpty(sign))
                {
                    _logger.LogWarning("签名验证失败: 未找到sign参数");
                    return false;
                }

                if (!paramsDict.TryGetValue("sign_type", out string? signType))
                {
                    signType = "RSA2";
                }

                _logger.LogInformation("签名类型: {SignType}, 签名长度: {SignLength}", signType, sign.Length);

                var sortedParams = paramsDict
                    .Where(p => p.Key != "sign" && p.Key != "sign_type")
                    .OrderBy(p => p.Key)
                    .ToDictionary(p => p.Key, p => p.Value);

                string signContent = BuildSignString(sortedParams);

                _logger.LogInformation("待验签内容长度: {ContentLength}", signContent.Length);
                _logger.LogDebug("待验签内容前100字符: {Content}...", signContent[..Math.Min(100, signContent.Length)]);

                bool result = VerifyRSASignature(signContent, sign, _config.AlipayPublicKey, signType);

                _logger.LogInformation("RSA签名验证结果: {Result}", result ? "通过" : "失败");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证签名异常: {Message}", ex.Message);
                return false;
            }
        }

        private string GenerateSign(Dictionary<string, string> parameters)
        {
            var sortedParams = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);

            string signContent = BuildSignString(sortedParams);

            return RSASign(signContent, _config.PrivateKey, _config.SignType);
        }

        private string BuildSignString(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters.OrderBy(p => p.Key))
            {
                if (!string.IsNullOrEmpty(param.Value))
                {
                    sb.Append(param.Key).Append("=").Append(param.Value).Append("&");
                }
            }

            if (sb.Length > 0 && sb[sb.Length - 1] == '&')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        private string RSASign(string content, string privateKey, string signType)
        {
            try
            {
                using var rsa = RSA.Create();

                string key = privateKey;
                if (key.Contains("-----BEGIN"))
                {
                    key = key.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                               .Replace("-----END RSA PRIVATE KEY-----", "")
                               .Replace("-----BEGIN PRIVATE KEY-----", "")
                               .Replace("-----END PRIVATE KEY-----", "")
                               .Replace("\n", "")
                               .Replace("\r", "");
                }

                byte[] keyBytes = Convert.FromBase64String(key);

                try
                {
                    rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                }
                catch
                {
                    try
                    {
                        rsa.ImportRSAPrivateKey(keyBytes, out _);
                    }
                    catch
                    {
                        throw new CryptographicException("无法识别的私钥格式，请检查是否为有效的 RSA 私钥");
                    }
                }

                var dataBytes = Encoding.UTF8.GetBytes(content);

                byte[] signatureBytes;
                if (signType == "RSA2")
                {
                    signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                else
                {
                    signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                }

                return Convert.ToBase64String(signatureBytes);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "RSA签名失败 - 可能是私钥格式错误: {Message}", ex.Message);
                throw new CryptographicException($"RSA签名失败: {ex.Message}。请确认 PrivateKey 配置为正确的 RSA 私钥（不是公钥）");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RSA签名失败: {Message}", ex.Message);
                throw;
            }
        }

        private bool VerifyRSASignature(string content, string sign, string publicKey, string signType)
        {
            try
            {
                using var rsa = RSA.Create();

                string key = publicKey;
                if (key.Contains("-----BEGIN"))
                {
                    key = key.Replace("-----BEGIN PUBLIC KEY-----", "")
                               .Replace("-----END PUBLIC KEY-----", "")
                               .Replace("\n", "")
                               .Replace("\r", "");
                }

                byte[] keyBytes = Convert.FromBase64String(key);
                rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

                var dataBytes = Encoding.UTF8.GetBytes(content);
                var signBytes = Convert.FromBase64String(sign);

                bool result;
                if (signType == "RSA2")
                {
                    result = rsa.VerifyData(dataBytes, signBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                else
                {
                    result = rsa.VerifyData(dataBytes, signBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RSA验证签名失败");
                return false;
            }
        }

        private string BuildFormHtml(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<form id='alipaysubmit' action='" + _config.GatewayUrl + "' method='POST' style='display:none;'>");

            foreach (var param in parameters)
            {
                var encodedValue = param.Value
                    .Replace("&", "&amp;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#39;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
                sb.AppendLine($"<input type='hidden' name='{param.Key}' value='{encodedValue}' />");
            }

            sb.AppendLine("<script>document.getElementById('alipaysubmit').submit();</script>");
            sb.AppendLine("</form>");

            return sb.ToString();
        }
    }

    public class QueryTradeResult
    {
        public bool Success { get; set; }
        public string TradeNo { get; set; } = "";
        public string TradeStatus { get; set; } = "";
        public string TotalAmount { get; set; } = "";
        public string BuyerLogonId { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public bool IsPaid => TradeStatus == "TRADE_SUCCESS" || TradeStatus == "TRADE_FINISHED";
        public bool IsClosed => TradeStatus == "TRADE_CLOSED";
        public bool IsNotExist => TradeStatus == "TRADE_NOT_EXIST";
    }
}
