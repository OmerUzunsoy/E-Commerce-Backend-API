using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.API.Filters;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<CreateCategoryRequestDto>))]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<UpdateCategoryRequestDto>))]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await categoryService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
