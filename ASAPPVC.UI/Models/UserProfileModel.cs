using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    public class UserProfileModel
    {
        // Primary key and foreign key to AspNetUsers
        [Key, ForeignKey(nameof(User))]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public IdentityUser User { get; set; } = null!;

        // Personal info
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        // Computed property, not stored in DB
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Active flag
        public bool IsActive { get; set; } = true;

        // Preferences stored as JSON for flexibility
        public string PreferencesJson { get; set; } = "{}";
    }
}