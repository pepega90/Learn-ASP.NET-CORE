using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.ViewModels
{
    // Kita membuat UserRoleViewModel yang berisikan
    // UserId = untuk  mendapatkan user id
    // Username = untuk menampilkan nama user di UI
    // IsSelected = untuk memilih user yang diinginkan ke dalam role
    // kita bisa menambahkan RoleId, namun nantinya akan terjadi one-to-many relationship
    // maka dari itu untuk roleid kita nanti berikan lewat ViewBag
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool IsSelected { get; set; }
    }
}
