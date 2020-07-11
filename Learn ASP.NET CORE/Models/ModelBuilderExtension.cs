using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // kita memberikan inital data, dengan cara membuat extension method
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Sigit",
                    Department = Dpt.HR,
                    Email = "sigit@gmail.com"
                },
                 new Employee
                 {
                     Id = 2,
                     Name = "Aji",
                     Department = Dpt.IT,
                     Email = "aji@gmail.com"
                 }
                );
        }
    }
}
