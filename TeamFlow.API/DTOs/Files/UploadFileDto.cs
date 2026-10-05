using Microsoft.AspNetCore.Http;

namespace TeamFlow.API.DTOs.Files;

public class UploadFileDto
{
    public IFormFile File { get; set; } = null!;
}