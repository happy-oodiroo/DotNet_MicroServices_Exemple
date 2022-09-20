using System.ComponentModel.DataAnnotations;

namespace StudentInfo.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Unique Identifier is required")]
        public string? UniqueIdentifier { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
    public class LoginResponce
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
