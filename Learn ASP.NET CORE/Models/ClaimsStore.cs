using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    // class ini berguna untuk menyimpan claim, claim pada dasarnya semacam kita memberikan 
    // akses control ke user. seperti misalkan user A kita berikan akses untuk membuat post, tetapi kita 
    // tidak memberikan akses untuk edit. dan user B kita berikan akses untuk edit post, tetapi tidak memiliki akses
    // untuk membuat post (kurang lebih begitu)

    // classnya kita buat static agar kita tidak perlu membuat instance dari class ini
    public static class ClaimsStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("Create Role", "Create Role"),
            new Claim("Edit Role", "Edit Role"),
            new Claim("Delete Role", "Delete Role")
        };
    }
}
