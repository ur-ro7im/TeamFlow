using TeamFlow.API.DTOs.Files;

namespace TeamFlow.API.Services.Interfaces;

public interface IFileService
{
    Task<FileResponseDto> UploadAsync(
        int taskId,
        UploadFileDto dto);

    Task<List<FileResponseDto>> GetTaskFilesAsync(
        int taskId);

    Task DeleteAsync(int fileId);

    Task<(byte[] FileBytes, string ContentType, string FileName)>
    DownloadAsync(int fileId);
}