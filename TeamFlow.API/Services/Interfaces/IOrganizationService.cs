using TeamFlow.API.DTOs.Organizations;

namespace TeamFlow.API.Services.Interfaces;

public interface IOrganizationService
{
    Task<List<MemberResponseDto>> GetMembersAsync();

    Task AddMemberAsync(AddMemberDto dto);

    Task RemoveMemberAsync(int userId);

    Task UpdateMemberRoleAsync(
        int userId,
        UpdateMemberRoleDto dto);

    Task<StorageResponseDto> GetStorageAsync();
}