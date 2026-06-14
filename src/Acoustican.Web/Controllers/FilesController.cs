using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager")]
public class FilesController(IFileUploadService fileUploadService) : ControllerBase
{
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        var response = await fileUploadService.UploadImageAsync(file);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("upload-video")]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        var response = await fileUploadService.UploadVideoAsync(file);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("upload-audio")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        var response = await fileUploadService.UploadAudioAsync(file);
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
    {
        var result = await fileUploadService.DeleteFileAsync(filePath);
        if (!result)
            return NotFound(new { message = "File not found" });
        return Ok(new { message = "File deleted successfully" });
    }
}
