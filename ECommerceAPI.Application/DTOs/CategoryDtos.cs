namespace ECommerceAPI.Application.DTOs;

public sealed record CategoryDto(Guid Id, string Name, string Description);

public sealed record CreateCategoryRequestDto(string Name, string Description);

public sealed record UpdateCategoryRequestDto(string Name, string Description);
