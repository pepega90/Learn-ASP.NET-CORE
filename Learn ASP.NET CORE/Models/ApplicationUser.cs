using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    /* kita membuat custom class yaitu ApplicationUser, dimana class ini akan berisikan informasi tambahan
      dari user. karena IdentityUser class memiliki properti yang terbatas. maka dari itu kita bisa melakukan
     inherit ke IdentityUser class untuk menambahan properti-properti tersebut*/
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
    }
}
