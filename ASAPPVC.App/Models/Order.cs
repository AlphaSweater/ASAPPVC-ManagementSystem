using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    [Index(nameof(OrderCode), IsUnique = true)]
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(64)]
        public string OrderCode { get; set; } = string.Empty;

        [Required, ForeignKey(nameof(Customer))]
        public Guid CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public Customer? Customer { get; set; }
        public List<OrderProduct> OrderProducts { get; set; } = new();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Completed,
        Cancelled
    }
}