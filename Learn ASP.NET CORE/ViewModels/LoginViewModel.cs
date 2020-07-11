using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
        
        // Properti dimana, nantinya user kembali ke url aplikasi kita setelah login di akun google
        public string ReturnUrl { get; set; }

        // Properti yang berisikan semua external login kita, misal facebook, google, twitter etc
        // untuk saat ini kita hanya memiliki google
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
