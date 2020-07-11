using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Learn_ASP.NET_CORE.Models
{
    // IdentityDbContext class, untuk mengatur akun user. dimana kita bisa create, read, update ,delete akun user. dan sebagainya
    // IdentityDbContext class sudah inherit dari DbContext, maka dari itu kita tidak perlu melakukan inherit secara explicit lagi ke DbContext
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        /* Instance dari DbContextOptions membawa informasi konfigurasi seperti string koneksi, 
         * penyedia database untuk digunakan dll.
         */
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {

        }
        // Properti untuk setiap entity di model, saat ini kita hanya punya satu entity class yaitu employee
        // properti Employees, nantiny kita gunakan untuk queries dan save changes pada employee class
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();

            // Mengubah entity framework core delete behavior
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
