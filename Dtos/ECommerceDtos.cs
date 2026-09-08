namespace ECommerceApi.Dtos;

// Customer DTOs
public record CustomerCreateDto(string FirstName, string LastName, string Email);
public record CustomerResponseDto(int CustomerId, string FirstName, string LastName, string Email, DateTime CreatedAt);

// Product DTOs
public record ProductCreateDto(string SKU, string Name, string? Description, decimal Price, int InitialStock);
public record ProductResponseDto(int ProductId, string SKU, string Name, string? Description, decimal Price, bool IsActive, int StockQuantity);

// Inventory DTOs
public record InventoryUpdateDto(int StockQuantity);
public record InventoryResponseDto(int ProductId, string ProductName, int Quantity);

// Order DTOs
public record OrderItemRequestDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<OrderItemRequestDto> Items);
public record OrderItemResponseDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderResponseDto(int OrderId, int CustomerId, DateTime OrderDate, string Status, decimal TotalAmount, List<OrderItemResponseDto> Items);
