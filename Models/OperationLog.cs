using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

/// <summary>
/// 操作日志实体，对应数据库 operation_logs 表
/// 记录系统中所有关键操作的审计日志，包括登录、注册、审核、数据变更等
/// </summary>
[Table("operation_logs")]
public class OperationLog
{
    /// <summary>
    /// 日志主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 操作类型，可选值：login-登录、logout-退出、register-注册、security-安全、audit-审核、user_manage-用户管理、system-系统、announcement-公告、banner-横幅
    /// </summary>
    [Column("type")]
    [MaxLength(20)]
    public string Type { get; set; } = "system";

    /// <summary>
    /// 操作人用户ID，未登录操作时为null
    /// </summary>
    [Column("operator_id")]
    public long? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    [Column("operator_name")]
    [MaxLength(50)]
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作描述，记录操作的详细说明
    /// </summary>
    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 操作者IP地址
    /// </summary>
    [Column("ip_address")]
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// IP地址对应的地理位置，通过IP定位服务获取
    /// </summary>
    [Column("location")]
    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>
    /// 操作者使用的浏览器，从User-Agent解析
    /// </summary>
    [Column("browser")]
    [MaxLength(100)]
    public string? Browser { get; set; }

    /// <summary>
    /// 操作者使用的操作系统，从User-Agent解析
    /// </summary>
    [Column("os")]
    [MaxLength(100)]
    public string? Os { get; set; }

    /// <summary>
    /// 请求URL路径
    /// </summary>
    [Column("request_url")]
    [MaxLength(500)]
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求HTTP方法，如 GET、POST、PUT、DELETE
    /// </summary>
    [Column("request_method")]
    [MaxLength(10)]
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求参数，JSON格式，最多存储2000字符
    /// </summary>
    [Column("request_params")]
    public string? RequestParams { get; set; }

    /// <summary>
    /// HTTP响应状态码，如200、404、500等
    /// </summary>
    [Column("response_code")]
    public int? ResponseCode { get; set; }

    /// <summary>
    /// 操作状态，可选值：success-成功、fail-失败
    /// </summary>
    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "success";

    /// <summary>
    /// 错误信息，操作失败时记录具体的错误原因
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 请求耗时，单位为毫秒
    /// </summary>
    [Column("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// 日志创建时间，即操作发生时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
