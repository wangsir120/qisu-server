using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace qisu_server.Controllers;

/// <summary>
/// 文件上传控制器
/// </summary>
[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadController> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".ico" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 上传普通图片
    /// </summary>
    /// <param name="file">图片文件</param>
    /// <returns>上传结果，包含图片URL</returns>
    /// <remarks>
    /// 支持的图片格式：jpg、jpeg、png、gif、webp
    /// 文件大小限制：5MB
    /// </remarks>
    [HttpPost("image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "请选择要上传的文件" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { success = false, message = "文件大小不能超过5MB" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = "只支持 jpg、png、gif、webp 格式的图片" });
        }

        try
        {
            var uploadDir = Path.Combine(_environment.ContentRootPath, "uploads", "images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/images/{fileName}";

            _logger.LogInformation("文件上传成功: {FileName}", fileName);

            return Ok(new { success = true, url, message = "上传成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败");
            return StatusCode(500, new { success = false, message = "上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传轮播图
    /// </summary>
    /// <param name="file">图片文件</param>
    /// <returns>上传结果，包含图片URL</returns>
    /// <remarks>
    /// 支持的图片格式：jpg、jpeg、png、gif、webp
    /// 文件大小限制：5MB
    /// </remarks>
    [HttpPost("banner")]
    [Authorize]
    public async Task<IActionResult> UploadBanner(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "请选择要上传的文件" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { success = false, message = "文件大小不能超过5MB" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = "只支持 jpg、png、gif、webp 格式的图片" });
        }

        try
        {
            var uploadDir = Path.Combine(_environment.ContentRootPath, "uploads", "banners");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/banners/{fileName}";

            _logger.LogInformation("轮播图上传成功: {FileName}", fileName);

            return Ok(new { success = true, url, message = "上传成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "轮播图上传失败");
            return StatusCode(500, new { success = false, message = "上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传网站图标
    /// </summary>
    /// <param name="file">图标文件</param>
    /// <returns>上传结果，包含图标URL</returns>
    /// <remarks>
    /// 支持的图片格式：jpg、jpeg、png、gif、webp、ico
    /// 文件大小限制：5MB
    /// </remarks>
    [HttpPost("icon")]
    [Authorize]
    public async Task<IActionResult> UploadIcon(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "请选择要上传的文件" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { success = false, message = "文件大小不能超过5MB" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = "只支持 jpg、png、gif、webp、ico 格式的图片" });
        }

        try
        {
            var uploadDir = Path.Combine(_environment.ContentRootPath, "uploads", "icons");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = $"favicon{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/icons/{fileName}";

            _logger.LogInformation("网站图标上传成功: {FileName}", fileName);

            return Ok(new { success = true, url, message = "上传成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "网站图标上传失败");
            return StatusCode(500, new { success = false, message = "上传失败，请稍后重试" });
        }
    }
}
