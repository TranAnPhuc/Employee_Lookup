using Employee_Lookup.Models;

namespace Employee_Lookup.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployeesAsync();

        Task<List<Employee>> SearchByNameAsync(string name);

        Task<List<Employee>> SearchByEmployeeCodeAsync(string employeeCode);

        Task<List<Employee>> SearchByDepartmentCodeAsync(string departmentCode);

        Task<Employee> GetEmployeeByIdAsync(int id);

        Task<bool> UpdateEmployeeAsync(string employeeCode, Employee employee);

        Task<ApiResponse> AddEmployeeAsync(AddEmployee employee);
    }
}