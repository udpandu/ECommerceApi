using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Inventory)
            .Select(p => new ProductResponseDto(p.ProductId, p.SKU, p.Name, p.Description, p.Price, p.IsActive, p.Inventory != null ? p.Inventory.Quantity : 0))
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var p = await _context.Products.Include(p => p.Inventory).FirstOrDefaultAsync(x => x.ProductId == id);
        if (p == null) return NotFound();

        return Ok(new ProductResponseDto(p.ProductId, p.SKU, p.Name, p.Description, p.Price, p.IsActive, p.Inventory?.Quantity ?? 0));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductCreateDto dto)
    {
        if (await _context.Products.AnyAsync(p => p.SKU == dto.SKU))
            return BadRequest("Product SKU already exists.");

        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Inventory = new Inventory { Quantity = dto.InitialStock }
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId },
            new ProductResponseDto(product.ProductId, product.SKU, product.Name, product.Description, product.Price, product.IsActive, product.Inventory.Quantity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
