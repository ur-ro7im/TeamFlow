using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Tasks;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(
        ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetTask(
        int taskId)
    {
        var task =
            await _taskService
                .GetTaskByIdAsync(taskId);

        return Ok(task);
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetProjectTasks(
     int projectId,
     [FromQuery] TaskFilterDto filter)
    {
        var tasks =
            await _taskService
                .GetProjectTasksAsync(
                    projectId,
                    filter);

        return Ok(tasks);
    }

    [HttpPost("project/{projectId:int}")]
    public async Task<IActionResult> CreateTask(
        int projectId,
        CreateTaskDto dto)
    {
        var task =
            await _taskService
                .CreateTaskAsync(
                    projectId,
                    dto);

        return CreatedAtAction(
            nameof(GetTask),
            new
            {
                taskId = task.Id
            },
            task);
    }

    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> UpdateTask(
        int taskId,
        UpdateTaskDto dto)
    {
        await _taskService
            .UpdateTaskAsync(
                taskId,
                dto);

        return NoContent();
    }

    [HttpPatch("{taskId:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int taskId,
        UpdateTaskStatusDto dto)
    {
        await _taskService
            .UpdateTaskStatusAsync(
                taskId,
                dto);

        return NoContent();
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeleteTask(
        int taskId)
    {
        await _taskService
            .DeleteTaskAsync(taskId);

        return NoContent();
    }
}