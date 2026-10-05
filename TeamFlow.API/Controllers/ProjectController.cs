using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Project;
using TeamFlow.API.DTOs.Projects;
using TeamFlow.API.Interfaces;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectService.GetAllAsync();

        return Ok(projects);
    }

    // GET: api/projects/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project is null)
            return NotFound(new
            {
                message = "Project not found."
            });

        return Ok(project);
    }

    // POST: api/projects
    [HttpPost]
    [Authorize(Policy = "AdminOrOwner")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectDto dto)
    {
        var project =
            await _projectService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            project
        );
    }

    // PUT: api/projects/5
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOwner")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProjectDto dto)
    {
        var project =
            await _projectService.UpdateAsync(id, dto);

        if (project is null)
            return NotFound(new
            {
                message = "Project not found."
            });

        return Ok(project);
    }

    // DELETE: api/projects/5
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOrOwner")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _projectService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new
            {
                message = "Project not found."
            });

        return NoContent();
    }
}