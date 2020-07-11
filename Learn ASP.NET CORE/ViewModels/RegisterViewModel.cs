using Learn_ASP.NET_CORE.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        // agar bisa melakukan remove validation, kita gunakan Remove attribute
        // dan berikan properti action dan controllernya
        [Remote(action: "IsEmailSudahDigunakan", controller: "Account")]
        [ValidEmailDomain(allowedDomain: "ajihandsome.com", 
            ErrorMessage = "Domain email harus ajihandsome.com")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password dan Confirm Password tidak sama!")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }

    }
}
