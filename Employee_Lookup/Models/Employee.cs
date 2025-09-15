using System.ComponentModel.DataAnnotations;

namespace Employee_Lookup.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Display(Name = "Mã nhân sự")]
        public string employeeCode { get; set; }

        [Display(Name = "Tên nhân sự")]
        public string employeeName { get; set; }

        [Display(Name = "Mã bộ phận")]
        public string departmentCode { get; set; }

        [Display(Name = "Email")]
        public string email { get; set; }
    }
}