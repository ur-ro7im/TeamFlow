using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Projects;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class ProjectMemberService
    : IProjectMemberService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProjectMemberService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task AddMemberAsync(
        int projectId,
        AddProjectMemberDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var project =
            await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.Id == projectId &&
                    x.OrganizationId == organizationId);

        if (project == null)
        {
            throw new NotFoundException(
                "Project was not found.");
        }

        var organizationMember =
            await _context.OrganizationMembers
                .AnyAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == dto.UserId);

        if (!organizationMember)
        {
            throw new BadRequestException(
                "User is not a member of this organization.");
        }

        var alreadyMember =
            await _context.ProjectMembers
                .AnyAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == dto.UserId);

        if (alreadyMember)
        {
            throw new ConflictException(
                "User is already a member of this project.");
        }

        var projectMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId,
            JoinedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);

        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(
    int projectId,
    int userId)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var project =
            await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.Id == projectId &&
                    x.OrganizationId == organizationId);

        if (project == null)
        {
            throw new NotFoundException(
                "Project was not found.");
        }

        var projectMember =
            await _context.ProjectMembers
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == userId);

        if (projectMember == null)
        {
            throw new NotFoundException(
                "User is not a member of this project.");
        }

        _context.ProjectMembers.Remove(projectMember);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProjectMemberResponseDto>>
    GetMembersAsync(int projectId)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var projectExists =
            await _context.Projects
                .AnyAsync(x =>
                    x.Id == projectId &&
                    x.OrganizationId == organizationId);

        if (!projectExists)
        {
            throw new NotFoundException(
                "Project was not found.");
        }

        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(x =>
                x.ProjectId == projectId)
            .Select(x => new ProjectMemberResponseDto
            {
                UserId = x.UserId,
                FullName = x.User.FullName,
                Email = x.User.Email,
                JoinedAt = x.JoinedAt
            })
            .ToListAsync();
    }
}