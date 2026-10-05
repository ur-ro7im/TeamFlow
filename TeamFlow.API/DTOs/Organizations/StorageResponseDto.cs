namespace TeamFlow.API.DTOs.Organizations;

public class StorageResponseDto
{
    public long UsedStorage { get; set; }

    public long MaxStorage { get; set; }

    public double UsedPercentage { get; set; }
}