using ASAPPVC.UI.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    // Identity user with Guid key and merged profile fields
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Personal info
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public bool IsActive { get; set; } = true;

        // Flexible preferences storage
        public string PreferencesJson { get; set; } = "{}";

        // Factory helper: create ApplicationUser from RegisterViewModel
        public static ApplicationUser Create(RegisterViewModel vm, bool emailConfirmed = false)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = vm.Email,
                Email = vm.Email,
                FirstName = vm.FirstName ?? string.Empty,
                LastName = vm.LastName ?? string.Empty,
                IsActive = true,
                PreferencesJson = "{}",
                EmailConfirmed = emailConfirmed
            };
        }
    }

    public enum RoleType
    {
        Admin,
        WarehouseManager,
        Unassigned
    }
}