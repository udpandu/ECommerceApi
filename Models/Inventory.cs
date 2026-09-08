using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Models;

public class Inventory
{
    [Key]
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();

    public Product Product { get; set; } = null!;
}
