namespace ASAPPVC.App.Models.Enums
{
    /// <summary>
    /// UI metadata for <see cref="ReorderStatus"/>.
    /// Keep this to presentation concerns only (no thresholds/rules here).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReorderStatusInfoAttribute(string label, string cssClass, string color) : Attribute
    {
        public string Label { get; } = label;
        public string CssClass { get; } = cssClass;
        public string Color { get; } = color;
    }

    /// <summary>
    /// Describes how close stock is to its reorder level.
    /// </summary>
    public enum ReorderStatus
    {
        [ReorderStatusInfo("Overstock", "status-good", "#2ecc71")]
        Overstock = 0,

        [ReorderStatusInfo("Healthy", "status-ok", "#27ae60")]
        Healthy = 1,

        [ReorderStatusInfo("Approaching", "status-warn", "#f1c40f")]
        Approaching = 2,

        [ReorderStatusInfo("Low", "status-low", "#e67e22")]
        Low = 3,

        [ReorderStatusInfo("Critical", "status-critical", "#e74c3c")]
        Critical = 4,

        [ReorderStatusInfo("Out of stock", "status-out", "#c0392b")]
        OutOfStock = 5
    }

    /// <summary>
    /// Single source of truth for reorder classification rules.
    /// Adjust thresholds here without touching enums/VMs/views.
    /// </summary>
    public static class ReorderStatusPolicy
    {
        /// <summary>
        /// Ratio = QuantityOnHand / ReorderLevel (when ReorderLevel &gt; 0).
        /// </summary>
        public const decimal Critical = 0.25m;

        public const decimal Low = 0.50m;
        public const decimal Approaching = 0.75m;
        public const decimal Healthy = 1.00m;
        public const decimal Overstock = 1.50m;

        /// <summary>
        /// Evaluate status from current quantity and configured reorder level.
        /// Treats non-positive reorder levels as "no threshold", returning Healthy unless quantity is zero.
        /// </summary>
        public static ReorderStatus Evaluate(decimal quantityOnHand, decimal reorderLevel)
        {
            if (reorderLevel <= 0)
                return quantityOnHand <= 0 ? ReorderStatus.OutOfStock : ReorderStatus.Healthy;

            if (quantityOnHand <= 0)
                return ReorderStatus.OutOfStock;

            var ratio = quantityOnHand / reorderLevel;

            if (ratio < Critical)
                return ReorderStatus.Critical;
            if (ratio < Low)
                return ReorderStatus.Low;
            if (ratio < Approaching)
                return ReorderStatus.Approaching;
            if (ratio < Overstock)
                return ReorderStatus.Healthy;
            return ReorderStatus.Overstock;
        }

        /// <summary>
        /// Convenience helper to get the ratio (QuantityOnHand/ReorderLevel). Returns decimal.MaxValue when no valid level.
        /// </summary>
        public static decimal GetRatio(decimal quantityOnHand, decimal reorderLevel)
        {
            return reorderLevel <= 0 ? decimal.MaxValue : (quantityOnHand / reorderLevel);
        }
    }
}