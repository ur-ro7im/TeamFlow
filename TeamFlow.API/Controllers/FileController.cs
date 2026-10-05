using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Files;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("task/{taskId:int}")]
    public async Task<IActionResult> UploadFile(
    int taskId,
    [FromForm] UploadFileDto dto)
    {
        var file = await _fileService
            .UploadAsync(taskId, dto);

        return Ok(file);
    }

    [HttpGet("task/{taskId:int}")]
    public async Task<IActionResult> GetTaskFiles(
    int taskId)
    {
        var files = await _fileService
            .GetTaskFilesAsync(taskId);

        return Ok(files);
    }

    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> DownloadFile(
    int fileId)
    {
        var result = await _fileService
            .DownloadAsync(fileId);

        return File(
            result.FileBytes,
            result.ContentType,
            result.FileName);
    }

    [HttpDelete("{fileId:int}")]
    public async Task<IActionResult> DeleteFile(
    int fileId)
    {
        await _fileService.DeleteAsync(fileId);

        return NoContent();
    }
}