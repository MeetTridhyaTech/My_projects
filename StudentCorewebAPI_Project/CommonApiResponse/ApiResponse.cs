﻿
namespace StudentCorewebAPI_Project.CommonApiResponse;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ApiResponse(bool success, string message, T data)
    {
        Success = success;
        Message = message;
        Data = data;
    }
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>(true, message, data);
    }
    public static ApiResponse<T> FailureResponse(string message, T data = default)
    {
        return new ApiResponse<T>(false, message, data);
    }
}
