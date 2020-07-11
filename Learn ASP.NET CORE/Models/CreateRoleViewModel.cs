using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Nama Role")]
        public string RoleName { get; set; }
    }
}
