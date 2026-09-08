using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context) => _context = context;

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
    {
        var o = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(x => x.OrderId == id);

        if (o == null) return NotFound();

        var items = o.OrderItems.Select(i => 
            new OrderItemResponseDto(i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList();

        return Ok(new OrderResponseDto(o.OrderId, o.CustomerId, o.OrderDate, o.Status, o.TotalAmount, items));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderDto dto)
    {
        if (!dto.Items.Any())
            return BadRequest("Order must contain at least one item.");

        var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
        if (!customerExists)
            return BadRequest($"Customer ID {dto.CustomerId} does not exist.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.ProductId)).ToDictionaryAsync(p => p.ProductId);
            var inventories = await _context.Inventories.Where(i => productIds.Contains(i.ProductId)).ToDictionaryAsync(i => i.ProductId);

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                    return BadRequest($"Product ID {item.ProductId} is unavailable.");

                if (!inventories.TryGetValue(item.ProductId, out var stock) || stock.Quantity < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for product '{product.Name}'. Available: {stock?.Quantity ?? 0}, Requested: {item.Quantity}");
                }

                stock.Quantity -= item.Quantity;
                stock.RowVersion = Guid.NewGuid().ToByteArray();

                decimal lineTotal = product.Price * item.Quantity;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId },
                new OrderResponseDto(order.OrderId, order.CustomerId, order.OrderDate, order.Status, order.TotalAmount,
                    orderItems.Select(i => new OrderItemResponseDto(i.ProductId, products[i.ProductId].Name, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList()));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Transaction Failed: {ex.Message}");
        }
    }
}
