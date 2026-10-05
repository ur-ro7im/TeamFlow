namespace TeamFlow.API.Models;

public class FileAttachment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UploadedByUserId { get; set; }

    public string FileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Path { get; set; } = null!;

    public DateTime CreatedAt { get; set; }


    // Navigation Properties

    public TaskItem Task { get; set; } = null!;

    public User UploadedByUser { get; set; } = null!;
}