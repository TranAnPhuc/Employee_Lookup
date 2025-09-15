using System.ComponentModel.DataAnnotations;

namespace Employee_Lookup.Models
{
    public class AddEmployee
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [StringLength(12, ErrorMessage = "Mã nhân viên không được vượt quá 12 ký tự")]
        [Display(Name = "Mã nhân viên")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhân viên là bắt buộc")]
        [StringLength(30, ErrorMessage = "Tên nhân viên không được vượt quá 30 ký tự")]
        [Display(Name = "Tên nhân viên")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
        [StringLength(5, ErrorMessage = "Mã phòng ban không được vượt quá 5 ký tự")]
        [Display(Name = "Mã phòng ban")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(50, ErrorMessage = "Email không được vượt quá 50 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}