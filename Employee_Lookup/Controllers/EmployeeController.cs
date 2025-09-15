using Employee_Lookup.Models;
using Employee_Lookup.Services;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Lookup.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SearchViewModel();

            // Load all employees by default
            var allEmployees = await _employeeService.GetAllEmployeesAsync();

            // Áp dụng sắp xếp mặc định
            allEmployees = ApplySorting(allEmployees, model.SortBy, model.SortDirection);

            // Set total records
            model.TotalRecords = allEmployees.Count;

            // Validate page number
            model.ValidatePageNumber();

            // Apply pagination
            var pagedEmployees = ApplyPagination(allEmployees, model.PageNumber, model.PageSize);

            model.SearchResults = pagedEmployees;
            model.SearchType = "Tất cả nhân sự";

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            List<Employee> employees = new List<Employee>();

            // Validate and set default values
            if (model.PageNumber < 1) model.PageNumber = 1;
            if (model.PageSize < 1) model.PageSize = 10;

            try
            {
                if (!string.IsNullOrWhiteSpace(model.employeeName))
                {
                    employees = await _employeeService.SearchByNameAsync(model.employeeName);
                    model.SearchType = $"Tìm theo tên: {model.employeeName}";
                }
                else if (!string.IsNullOrWhiteSpace(model.employeeCode))
                {
                    employees = await _employeeService.SearchByEmployeeCodeAsync(model.employeeCode);
                    model.SearchType = $"Tìm theo mã nhân sự: {model.employeeCode}";
                }
                else if (!string.IsNullOrWhiteSpace(model.departmentCode))
                {
                    employees = await _employeeService.SearchByDepartmentCodeAsync(model.departmentCode);
                    model.SearchType = $"Tìm theo mã phòng ban: {model.departmentCode}";
                }
                else
                {
                    employees = await _employeeService.GetAllEmployeesAsync();
                    model.SearchType = "Tất cả nhân sự";
                }

                // Áp dụng sắp xếp
                employees = ApplySorting(employees, model.SortBy, model.SortDirection);

                // Set total records
                model.TotalRecords = employees.Count;

                // Validate page number after getting total records
                model.ValidatePageNumber();

                // Apply pagination
                var pagedEmployees = ApplyPagination(employees, model.PageNumber, model.PageSize);
                model.SearchResults = pagedEmployees;

                _logger?.LogInformation($"Search completed. Found {model.TotalRecords} employees, showing page {model.PageNumber} of {model.TotalPages}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred during employee search");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tìm kiếm. Vui lòng thử lại.";

                // Return empty results on error
                model.SearchResults = new List<Employee>();
                model.TotalRecords = 0;
                model.PageNumber = 1;
            }

            return View("Index", model);
        }

        // Thêm action để reset sắp xếp về mặc định
        [HttpPost]
        public async Task<IActionResult> ResetSort(SearchViewModel model)
        {
            // Reset sắp xếp về mặc định
            model.ResetSort();
            model.ResetPaging();

            // Thực hiện lại search với sắp xếp mặc định
            return await Search(model);
        }

        // Method để xử lý sắp xếp
        private List<Employee> ApplySorting(List<Employee> employees, string sortBy, string sortDirection)
        {
            if (employees == null || !employees.Any())
                return employees ?? new List<Employee>();

            if (string.IsNullOrEmpty(sortBy))
                return employees;

            try
            {
                switch (sortBy.ToLower())
                {
                    case "employeename":
                        employees = sortDirection == "asc"
                            ? employees.OrderBy(x => x.employeeName ?? "").ToList()
                            : employees.OrderByDescending(x => x.employeeName ?? "").ToList();
                        break;

                    case "employeecode":
                        employees = sortDirection == "asc"
                            ? employees.OrderBy(x => x.employeeCode ?? "").ToList()
                            : employees.OrderByDescending(x => x.employeeCode ?? "").ToList();
                        break;

                    case "email":
                        employees = sortDirection == "asc"
                            ? employees.OrderBy(x => x.email ?? "").ToList()
                            : employees.OrderByDescending(x => x.email ?? "").ToList();
                        break;

                    case "departmentcode":
                        employees = sortDirection == "asc"
                            ? employees.OrderBy(x => x.departmentCode ?? "").ToList()
                            : employees.OrderByDescending(x => x.departmentCode ?? "").ToList();
                        break;

                    default:
                        // Mặc định sắp xếp theo tên
                        employees = employees.OrderBy(x => x.employeeName ?? "").ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error applying sorting. SortBy: {sortBy}, Direction: {sortDirection}");
                // Return original list if sorting fails
            }

            return employees;
        }

        // Method để xử lý phân trang
        private List<Employee> ApplyPagination(List<Employee> employees, int pageNumber, int pageSize)
        {
            if (employees == null || !employees.Any())
                return new List<Employee>();

            try
            {
                var skip = (pageNumber - 1) * pageSize;
                return employees.Skip(skip).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error applying pagination. PageNumber: {pageNumber}, PageSize: {pageSize}");
                return employees; // Return all if pagination fails
            }
        }

        // API endpoint để lấy thông tin phân trang
        [HttpGet]
        public IActionResult GetPaginationInfo(int totalRecords, int pageNumber, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var hasNextPage = pageNumber < totalPages;
            var hasPreviousPage = pageNumber > 1;

            return Json(new
            {
                totalPages = totalPages,
                currentPage = pageNumber,
                hasNextPage = hasNextPage,
                hasPreviousPage = hasPreviousPage,
                startRecord = totalRecords == 0 ? 0 : ((pageNumber - 1) * pageSize) + 1,
                endRecord = Math.Min(pageNumber * pageSize, totalRecords)
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                return Json(employees);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all employees");
                return Json(new { error = "Unable to retrieve employees" });
            }
        }

        // GET: Hiển thị form cập nhật nhân viên
        [HttpGet]
        public async Task<IActionResult> Edit(string employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                TempData["ErrorMessage"] = "Mã nhân viên không hợp lệ";
                return RedirectToAction("Index");
            }

            try
            {
                var employees = await _employeeService.SearchByEmployeeCodeAsync(employeeCode);
                var employee = employees.FirstOrDefault();

                if (employee == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với mã: {employeeCode}";
                    return RedirectToAction("Index");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error retrieving employee for editing. EmployeeCode: {employeeCode}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi truy xuất thông tin nhân viên";
                return RedirectToAction("Index");
            }
        }

        // POST: Cập nhật thông tin nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string employeeCode, Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                TempData["ErrorMessage"] = "Mã nhân viên không hợp lệ";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updateResult = await _employeeService.UpdateEmployeeAsync(employeeCode, employee);

                    if (updateResult)
                    {
                        TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin nhân viên";
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error updating employee. EmployeeCode: {employeeCode}");
                    TempData["ErrorMessage"] = "Có lỗi không mong muốn xảy ra khi cập nhật thông tin nhân viên";
                }
            }

            return View(employee);
        }

        // GET: Employee/Create - Hiển thị form thêm nhân viên
        [HttpGet]
        public IActionResult Create()
        {
            _logger?.LogInformation("Accessing Employee Create page");
            return View(new AddEmployee());
        }

        // POST: Employee/Create - Xử lý thêm nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddEmployee employee)
        {
            _logger?.LogInformation($"Attempting to create employee with code: {employee?.EmployeeCode}");

            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("Model validation failed for employee creation");

                // Log các lỗi validation để debug
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger?.LogWarning($"Validation error: {error.ErrorMessage}");
                }

                return View(employee);
            }

            try
            {
                var result = await _employeeService.AddEmployeeAsync(employee);

                if (result.Success)
                {
                    _logger?.LogInformation($"Successfully created employee with code: {employee.EmployeeCode}");
                    TempData["SuccessMessage"] = result.Message;

                    // Redirect về Index để thấy nhân viên vừa thêm
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger?.LogError($"Failed to create employee: {result.Message}");
                    TempData["ErrorMessage"] = result.Message;
                    return View(employee);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Exception occurred while creating employee with code: {employee?.EmployeeCode}");
                TempData["ErrorMessage"] = "Có lỗi không mong muốn xảy ra. Vui lòng thử lại sau.";
                return View(employee);
            }
        }

        // API endpoint để kiểm tra mã nhân viên có tồn tại hay không (cho validation phía client)
        [HttpGet]
        public async Task<IActionResult> CheckEmployeeCodeExists(string employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                return Json(new { exists = false });
            }

            try
            {
                var employees = await _employeeService.SearchByEmployeeCodeAsync(employeeCode);
                var exists = employees != null && employees.Any();

                _logger?.LogInformation($"Check employee code {employeeCode}: {(exists ? "exists" : "available")}");

                return Json(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error checking if employee code exists: {employeeCode}");
                return Json(new { exists = false, error = true });
            }
        }
    }
}