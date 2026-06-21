using Acoustican.DTOs;
using System.Linq;

namespace Acoustican.Services;

public interface IFileUploadService
{
    Task<FileUploadResponse> UploadImageAsync(IFormFile file, string directory = "images");
    Task<FileUploadResponse> UploadVideoAsync(IFormFile file, string directory = "videos");
    Task<FileUploadResponse> UploadAudioAsync(IFormFile file, string directory = "audio");
    Task<bool> DeleteFileAsync(string filePath);
}

public class FileUploadService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<FileUploadService> logger) : IFileUploadService
{
    public async Task<FileUploadResponse> UploadImageAsync(IFormFile file, string directory = "images")
    {
        return await UploadFileAsync(file, directory, configuration["FileUpload:AllowedImageExtensions"]!);
    }

    public async Task<FileUploadResponse> UploadVideoAsync(IFormFile file, string directory = "videos")
    {
        return await UploadFileAsync(file, directory, configuration["FileUpload:AllowedVideoExtensions"]!);
    }

    public async Task<FileUploadResponse> UploadAudioAsync(IFormFile file, string directory = "audio")
    {
        return await UploadFileAsync(file, directory, configuration["FileUpload:AllowedAudioExtensions"]!);
    }

    private async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string directory, string allowedExtensions)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new FileUploadResponse { Success = false, Message = "No file uploaded" };

            var maxFileSize = long.Parse(configuration["FileUpload:MaxFileSize"]!);
            if (file.Length > maxFileSize)
                return new FileUploadResponse { Success = false, Message = $"File size exceeds maximum limit of {maxFileSize / (1024 * 1024)}MB" };

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var allowedList = allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!allowedList.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                return new FileUploadResponse { Success = false, Message = $"File type not allowed. Allowed: {allowedExtensions}" };

            var uploadsPath = Path.Combine(environment.WebRootPath, configuration["FileUpload:UploadPath"]!, directory);
            Directory.CreateDirectory(uploadsPath);

            var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"/{configuration["FileUpload:UploadPath"]}/{directory}/{uniqueFileName}";

            logger.LogInformation("File uploaded successfully: {PublicPath}", publicPath);

            return new FileUploadResponse
            {
                Success = true,
                Message = "File uploaded successfully",
                FilePath = publicPath,
                FileName = uniqueFileName,
                FileSize = file.Length
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file");
            return new FileUploadResponse { Success = false, Message = $"Error uploading file: {ex.Message}" };
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var uploadPathSetting = configuration["FileUpload:UploadPath"] ?? "uploads";
            var uploadsRoot = Path.GetFullPath(Path.Combine(environment.WebRootPath, uploadPathSetting));
            
            var normalizedInput = filePath.Replace('\\', '/').TrimStart('/');
            var fullPath = Path.GetFullPath(Path.Combine(environment.WebRootPath, normalizedInput));

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Access denied/path traversal attempt detected for file deletion: {FilePath}", filePath);
                return false;
            }

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }
}
