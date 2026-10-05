//using TeamFlow.API.DTOs.Project;
using TeamFlow.API.DTOs.Project;
using TeamFlow.API.DTOs.Projects;

namespace TeamFlow.API.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponseDto>> GetAllAsync();

    Task<ProjectResponseDto?> GetByIdAsync(int id);

    Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto);

    Task<ProjectResponseDto?> UpdateAsync(
        int id,
        UpdateProjectDto dto);

    Task<bool> DeleteAsync(int id);
}