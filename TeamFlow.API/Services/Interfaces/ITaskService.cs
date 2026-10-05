using TeamFlow.API.DTOs.Tasks;

namespace TeamFlow.API.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResultDto<TaskResponseDto>>
    GetProjectTasksAsync(
        int projectId,
        TaskFilterDto filter);

    Task<TaskResponseDto> GetTaskByIdAsync(
        int taskId);

    Task<TaskResponseDto> CreateTaskAsync(
        int projectId,
        CreateTaskDto dto);

    Task UpdateTaskAsync(
        int taskId,
        UpdateTaskDto dto);

    Task UpdateTaskStatusAsync(
        int taskId,
        UpdateTaskStatusDto dto);

    Task DeleteTaskAsync(
        int taskId);
}