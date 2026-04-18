using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.API.Filters;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Models;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductQueryParameters query, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<CreateProductRequestDto>))]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<UpdateProductRequestDto>))]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await productService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
