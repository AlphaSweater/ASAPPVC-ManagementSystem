using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    public class UserProfileModel
    {
        [Key, ForeignKey(nameof(User))]
        [Required]
        public string UserID { get; set; } // PK and FK to AspNetUsers

        [Required]
        public IdentityUser User { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public bool IsActive { get; set; } = true;

        // Preferences (JSON for extensibility)
        public string PreferencesJson { get; set; }

        // Constructor for mapping RegisterViewModel
        //public UserProfileModel(RegisterViewModel model, IdentityUser user)
        //{
        //    UserID = user.Id;
        //    User = user;
        //    FirstName = model.FirstName;
        //    LastName = model.LastName;
        //    PreferencesJson = "{}";
        //}

        // Parameterless constructor for EF Core
        public UserProfileModel()
        { }
    }
}
