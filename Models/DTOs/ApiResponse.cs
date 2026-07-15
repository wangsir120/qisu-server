namespace qisu_server.Models.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int Code { get; set; } = 200;

    public static ApiResponse<T> Ok(T data, string message = "操作成功")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> Fail(string message, int code = 400)
    {
        return new ApiResponse<T> { Success = false, Message = message, Code = code };
    }
}
