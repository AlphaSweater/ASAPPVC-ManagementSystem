using ASAPPVC.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //-----------------------------------------------\\
    //  ProductComponent ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Component inside product (read-only view)
    //-----------------------------------------------\\
    /// <summary>
    /// Read-only representation of a product component used inside product detail views.<br/>
    /// Contains display fields and computed values useful for UI presentation.<br/>
    /// Use this VM only for display purposes inside a ProductDetailVm; it is not intended<br/>
    /// for create/edit form binding.<br/>
    /// </summary>
    public sealed class ProductComponentVm
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid ComponentId { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string ComponentName { get; init; } = string.Empty;
        public Unit UnitOfMeasure { get; init; }
        public decimal QuantityRequired { get; init; }
        public decimal UnitCost { get; init; }

        // Optional computed cost for UI
        public decimal TotalCost => UnitCost * QuantityRequired;

        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(QuantityRequired, true);
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add product)
    //-----------------------------------------------\\
    /// <summary>
    /// Component entry used within the create form. Represents the selected component and<br/>
    /// the required quantity. Use as items of CreateProductVm.Components when creating products.<br/>
    /// </summary>
    public sealed class CreateProductComponentVm
    {
        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Component")]
        [Required(ErrorMessage = "Component is required.")]
        public Guid ComponentId { get; set; }

        [Display(Name = "Unit of Measure")]
        [Required(ErrorMessage = "Unit of Measure is required.")]
        public Unit UnitOfMeasure { get; init; }

        [Display(Name = "Quantity Required")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, 999999, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal QuantityRequired { get; set; }
    }

    //-----------------------------------------------\\
    // Edit form (used in PUT / update product)
    //-----------------------------------------------\\
    /// <summary>
    /// Component entry used within the edit form. Includes an Id for the association (if persisted),<br/>
    /// the selected component, and quantity. Use as items of EditProductVm.Components for updates.<br/>
    /// </summary>
    public sealed class EditProductComponentVm
    {
        public Guid Id { get; set; }

        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Component")]
        [Required(ErrorMessage = "Component is required.")]
        public Guid ComponentId { get; set; }

        [Display(Name = "Unit of Measure")]
        [Required(ErrorMessage = "Unit of Measure is required.")]
        public Unit UnitOfMeasure { get; init; }

        [Display(Name = "Quantity Required")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, 999999, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal QuantityRequired { get; set; }
    }
}