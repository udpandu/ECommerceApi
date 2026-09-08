using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .Select(c => new CustomerResponseDto(c.CustomerId, c.FirstName, c.LastName, c.Email, c.CreatedAt))
            .ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomer(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null) return NotFound();

        return Ok(new CustomerResponseDto(c.CustomerId, c.FirstName, c.LastName, c.Email, c.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> CreateCustomer(CustomerCreateDto dto)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == dto.Email))
            return BadRequest("Customer with this Email already exists.");

        var customer = new Customer { FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId },
            new CustomerResponseDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerCreateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
