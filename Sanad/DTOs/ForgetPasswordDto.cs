using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sanad.DTOs
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        public string Email { get; set; }
    }    

}

