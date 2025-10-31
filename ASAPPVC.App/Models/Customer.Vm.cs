using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    // -----------------------------------------------\\
    // Customer ViewModels (Read + Write)
    // -----------------------------------------------\\

    // Summary used for lists, tables and search results
    public sealed class CustomerListVm
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Surname { get; init; } = string.Empty;

        public string Company { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;

        // Computed helpers
        public string FullName => string.IsNullOrWhiteSpace(Surname) ? Name : $"{Name} {Surname}";
    }

    // Detail view for a single customer
    public sealed class CustomerDetailVm
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Surname { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? Company { get; init; }

        // Audit / lifecycle
        public bool IsActive { get; init; } = true;

        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        // Computed helpers
        public string FullName => string.IsNullOrWhiteSpace(Surname) ? Name : $"{Name} {Surname}";

        public string DisplayEmail => Email;
    }

    // Form model used for create/edit pages
    public sealed class CustomerFormVm
    {
        public Guid? Id { get; set; }
        public bool IsEdit => Id.HasValue;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Surname { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Company { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Small helper for UI previews
        public string FullName => string.IsNullOrWhiteSpace(Surname) ? Name : $"{Name} {Surname}";
    }
}