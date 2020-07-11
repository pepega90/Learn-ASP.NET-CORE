using Learn_ASP.NET_CORE.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.ViewModels
{
    // Membuat Employee View model, untuk mengupload photo
    public class EmployeeCreateViewModel
    {

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
        // kita gunakan IFormFile interface, untuk mengupload gambar. 
        // IFormFile, memiliki beberapa properti dan method yang berguna untuk memudahkan kita dalam mengupload gambar
        // alasan mengapa IFormFile adalah, karena di class Employee kita nanti hanya akan menyimpan path ke file photo itu saja
        // jika kita ingin mengupload lebih dari 1 gambar, kita jadikan IFormFile sebagai List<IFormFile>
        public IFormFile Photo { get; set; }
    }
}
