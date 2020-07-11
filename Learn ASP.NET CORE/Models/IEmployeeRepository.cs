using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    // interface IEmployeeRepository , berisikan method
    // yang mengelola data karyawan seperti yang ditunjukkan di bawah ini.
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int id);
        IEnumerable<Employee> GetAllEmployee();
        Employee Add(Employee employee);
        Employee Update(Employee employeeChanges);
        Employee Delete(int id);
    }
}
