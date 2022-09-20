using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace StudentInfo.DTO
{
    public class ChangePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The string {0} must be at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm the new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and the confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
