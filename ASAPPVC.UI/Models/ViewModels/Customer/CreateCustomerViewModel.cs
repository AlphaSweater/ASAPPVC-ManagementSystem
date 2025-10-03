using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Customer
{
    public class CreateCustomerViewModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";

        [MaxLength(150)]
        public string? Company { get; set; }

        [Required, MaxLength(50)]
        public string PhoneNumber { get; set; } = "";

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = "";
    }
}