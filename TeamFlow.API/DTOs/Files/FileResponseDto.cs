namespace TeamFlow.API.DTOs.Files;

public class FileResponseDto
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UploadedByUserId { get; set; }

    public string UploadedByUserName { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public DateTime CreatedAt { get; set; }
}