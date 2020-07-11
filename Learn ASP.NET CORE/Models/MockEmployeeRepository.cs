using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Models
{
    // EmployeeRepository
    // berguna untuk mengelola data employee seperti yang ditunjukkan di bawah ini.
    // disini kita menggunakan MockEmployeeRepository sebagai data dummy,
    // Nantinya kita akan menggunakan data di database
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
                {
                    new Employee() { Id = 1, Name = "Mary", Department = Dpt.HR, Email = "mary@pragimtech.com" },
                    new Employee() { Id = 2, Name = "John", Department = Dpt.IT, Email = "john@pragimtech.com" },
                    new Employee() { Id = 3, Name = "Sam", Department = Dpt.IT, Email = "sam@pragimtech.com" },
                };
        }

        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employee = _employeeList.FirstOrDefault(emp => emp.Id == id);
            if (employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(emp => emp.Id == id);
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employee = _employeeList.FirstOrDefault(emp => emp.Id == employeeChanges.Id);
            if (employee != null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }
            return employee;
        }
    }
}
