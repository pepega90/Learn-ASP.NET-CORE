using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    /* Ada beberapa buillt in validation attribute yang bisa kita gunakan di ASP.NET Core
     * RegularExpresssion, Memvalidasi jika nilai yang diberikan cocok dengan pola yang ditentukan oleh ekspresi reguler
     * Required, Menentukan bidang yang harus diisi
     * Range, Menentukan nilai minimum dan maksimum yang diizinkan
     * MinLength, Specifies the minimum length of a string
     * MaxLength, Specifies the maximum length of a string
     * Compare, Membandingkan 2 properti model. Misalnya membandingkan properti Email dan ConfirmEmail
     */
    public class Employee
    {
        public int Id { get; set; }
        [NotMapped]
        public string EnkripsiId { get; set; }
        [Required(ErrorMessage = "Nama Wajib Diisi"), 
        MaxLength(50, ErrorMessage = "Nama Hanya boleh berisikan 50 karakter")] // <= kita membuat properti name menjadi wajib diisi (required), kita bisa memberikan custom pesan error dengan menggunakan error message properti
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
        ErrorMessage = "Invalid email format")]
        [Display(Name = "Office Email")]
        public string Email { get; set; }
        [Required]
        public Dpt? Department { get; set; }
        public string PhotoPath { get; set; }

    }
}
