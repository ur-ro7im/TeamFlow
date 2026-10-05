using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Files;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class FileService : IFileService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".txt"] = ["text/plain"],
            [".doc"] = ["application/msword"],
            [".docx"] =
            [
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            ],
            [".xls"] = ["application/vnd.ms-excel"],
            [".xlsx"] =
            [
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            ]
        };

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".png",
            ".jpg",
            ".jpeg",
            ".txt"
        };

    public FileService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IWebHostEnvironment environment)
    {
        _context = context;
        _currentUser = currentUser;
        _environment = environment;
    }

    public async Task<FileResponseDto> UploadAsync(
        int taskId,
        UploadFileDto dto)
    {
        var userId = _currentUser.UserId;
        var organizationId = _currentUser.OrganizationId;

        if (dto.File == null || dto.File.Length == 0)
        {
            throw new BadRequestException(
                "File is required.");
        }

        if (dto.File.Length > MaxFileSize)
        {
            throw new BadRequestException(
                "File size cannot exceed 10 MB.");
        }

        var task = await _context.Tasks
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                x.Project.OrganizationId == organizationId);

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        await EnsureProjectAccessAsync(
            task.ProjectId,
            userId);

        var extension = Path
            .GetExtension(dto.File.FileName)
            .ToLowerInvariant();

        ValidateFileType(
            extension,
            dto.File.ContentType);

        await ValidateFileSignatureAsync(
            dto.File,
            extension);

        var subscription = await _context.Subscriptions
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x =>
                x.OrganizationId == organizationId &&
                x.Status == SubscriptionStatus.Active);

        if (subscription == null)
        {
            throw new ConflictException(
                "Organization does not have an active subscription.");
        }

        var usedStorage = await _context.Files
            .Where(x =>
                x.Task.Project.OrganizationId == organizationId)
            .SumAsync(x => x.Size);

        var newStorage = usedStorage + dto.File.Length;

        if (subscription.Plan.MaxStorage != -1 &&
            newStorage > subscription.Plan.MaxStorage)
        {
            throw new ConflictException(
                "Storage limit exceeded.");
        }

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath = Path.Combine(
            uploadsFolder,
            storedFileName);

        try
        {
            await using var stream = new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await dto.File.CopyToAsync(stream);

            var fileAttachment = new FileAttachment
            {
                TaskId = taskId,
                UploadedByUserId = userId,

                // Original file name shown to the user
                FileName = Path.GetFileName(dto.File.FileName),

                // Random name used on disk
                StoredFileName = storedFileName,

                ContentType = dto.File.ContentType,
                Size = dto.File.Length,

                // Store relative path in DB
                Path = Path.Combine(
                    "uploads",
                    storedFileName),

                CreatedAt = DateTime.UtcNow
            };

            _context.Files.Add(fileAttachment);

            // The assignee and creator hear about new attachments (never the uploader).
            var recipients = new HashSet<int>();

            if (task.AssignedToUserId.HasValue)
            {
                recipients.Add(task.AssignedToUserId.Value);
            }

            recipients.Add(task.CreatedByUserId);
            recipients.Remove(userId);

            if (recipients.Count > 0)
            {
                var uploaderName = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.FullName)
                    .FirstAsync();

                foreach (var recipientId in recipients)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = recipientId,
                        Message =
                            $"{uploaderName} attached '{fileAttachment.FileName}' to the task '{task.Title}'.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await _context.Files
                .AsNoTracking()
                .Where(x => x.Id == fileAttachment.Id)
                .Select(x => new FileResponseDto
                {
                    Id = x.Id,
                    TaskId = x.TaskId,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByUserName =
                        x.UploadedByUser.FullName,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    Size = x.Size,
                    CreatedAt = x.CreatedAt
                })
                .FirstAsync();
        }
        catch
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            throw;
        }
    }

    public async Task<List<FileResponseDto>> GetTaskFilesAsync(
        int taskId)
    {
        var userId = _currentUser.UserId;
        var organizationId = _currentUser.OrganizationId;

        var task = await _context.Tasks
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                x.Project.OrganizationId == organizationId);

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        await EnsureProjectAccessAsync(
            task.ProjectId,
            userId);

        return await _context.Files
            .AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FileResponseDto
            {
                Id = x.Id,
                TaskId = x.TaskId,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByUserName =
                    x.UploadedByUser.FullName,
                FileName = x.FileName,
                ContentType = x.ContentType,
                Size = x.Size,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<
        (byte[] FileBytes, string ContentType, string FileName)>
        DownloadAsync(int fileId)
    {
        var userId = _currentUser.UserId;
        var organizationId = _currentUser.OrganizationId;

        var file = await _context.Files
            .Include(x => x.Task)
                .ThenInclude(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == fileId &&
                x.Task.Project.OrganizationId == organizationId);

        if (file == null)
        {
            throw new NotFoundException(
                "File not found.");
        }

        await EnsureProjectAccessAsync(
            file.Task.ProjectId,
            userId);

        var physicalPath = GetPhysicalPath(file.Path);

        if (!File.Exists(physicalPath))
        {
            throw new NotFoundException(
                "Physical file not found.");
        }

        var fileBytes =
            await File.ReadAllBytesAsync(physicalPath);

        return (
            fileBytes,
            file.ContentType,
            file.FileName);
    }

    public async Task DeleteAsync(int fileId)
    {
        var userId = _currentUser.UserId;
        var organizationId = _currentUser.OrganizationId;

        var file = await _context.Files
            .Include(x => x.Task)
                .ThenInclude(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == fileId &&
                x.Task.Project.OrganizationId == organizationId);

        if (file == null)
        {
            throw new NotFoundException(
                "File not found.");
        }

        await EnsureProjectAccessAsync(
            file.Task.ProjectId,
            userId);

        if (file.UploadedByUserId != userId &&
            _currentUser.Role == "Member")
        {
            throw new ForbiddenException(
                "You can only delete files you uploaded.");
        }

        var physicalPath = GetPhysicalPath(file.Path);

        _context.Files.Remove(file);

        await _context.SaveChangesAsync();

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }

    private async Task EnsureProjectAccessAsync(
        int projectId,
        int userId)
    {
        // Owner/Admin can access directly.
        if (_currentUser.Role != "Member")
        {
            return;
        }

        var isProjectMember =
            await _context.ProjectMembers.AnyAsync(x =>
                x.ProjectId == projectId &&
                x.UserId == userId);

        if (!isProjectMember)
        {
            throw new ForbiddenException(
                "You are not a member of this project.");
        }
    }

    private static void ValidateFileType(
        string extension,
        string contentType)
    {
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException(
                "This file type is not allowed.");
        }

        if (!AllowedContentTypes.TryGetValue(
                extension,
                out var allowedTypes))
        {
            throw new BadRequestException(
                "File type is not supported.");
        }

        if (!allowedTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Invalid file content type.");
        }
    }

    private static async Task ValidateFileSignatureAsync(
        IFormFile file,
        string extension)
    {
        // Binary formats with a well-known header are checked against it.
        // Text-like and legacy Office formats have no reliable short signature.
        byte[]? signature = extension switch
        {
            ".pdf" => new byte[] { 0x25, 0x50, 0x44, 0x46 },
            ".png" => new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            ".jpg" or ".jpeg" => new byte[] { 0xFF, 0xD8, 0xFF },
            ".docx" or ".xlsx" => new byte[] { 0x50, 0x4B },
            _ => null
        };

        if (signature is null)
        {
            return;
        }

        var header = new byte[signature.Length];

        await using var stream = file.OpenReadStream();

        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));

        if (read < signature.Length ||
            !header.AsSpan().SequenceEqual(signature))
        {
            throw new BadRequestException(
                "The file content does not match its type.");
        }
    }

    private string GetPhysicalPath(string relativePath)
    {
        var normalizedPath = relativePath
            .Replace(
                '/',
                Path.DirectorySeparatorChar)
            .Replace(
                '\\',
                Path.DirectorySeparatorChar);

        var webRootPath = Path.GetFullPath(
            _environment.WebRootPath);

        var physicalPath = Path.GetFullPath(
            Path.Combine(
                webRootPath,
                normalizedPath));

        // Prevent path traversal outside wwwroot.
        if (!physicalPath.StartsWith(
                webRootPath + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "Invalid file path.");
        }

        return physicalPath;
    }
}