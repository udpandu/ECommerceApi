using ECommerceApi.Data;
using ECommerceApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context) => _context = context;

    [HttpGet("{productId}")]
    public async Task<ActionResult<InventoryResponseDto>> GetStock(int productId)
    {
        var stock = await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (stock == null) return NotFound("Product not found in inventory.");

        return Ok(new InventoryResponseDto(stock.ProductId, stock.Product.Name, stock.Quantity));
    }

    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateStock(int productId, InventoryUpdateDto dto)
    {
        var stock = await _context.Inventories.FindAsync(productId);
        if (stock == null) return NotFound("Inventory item not found.");

        stock.Quantity = dto.StockQuantity;
        stock.RowVersion = Guid.NewGuid().ToByteArray();

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
