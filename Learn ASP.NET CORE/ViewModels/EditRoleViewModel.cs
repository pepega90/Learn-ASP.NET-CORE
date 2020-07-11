using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Role Name Harus diisi!")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; } = new List<string>();
    }
}
