namespace ASAPPVC.App.Models.Enums
{
    /// <summary>
    /// Order status values used by the Order entity.
    /// Extracted from Order.cs to its own file for reusability.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Picked,
        Shipped,
        Completed,
        Cancelled
    }
}