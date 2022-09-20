using System.ComponentModel.DataAnnotations;

namespace StudentInfo.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Unique Identifier is required")]
        public string UniqueIdentifier { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string AddressLines { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip Code is required")]
        public string ZipCode { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
