using Employee_Lookup.Models;
using Octokit.Internal;
using Refit;
using System.Text.Json;

namespace Employee_Lookup.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeService> _logger;
        private readonly string _apiBaseUrl;

        public EmployeeService(HttpClient httpClient, IConfiguration configuration, ILogger<EmployeeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        }

        public async Task<ApiResponse> AddEmployeeAsync(AddEmployee employee)
        {
            try
            {
                _logger.LogInformation($"Thêm nhân sự mới với mã nhân sự: {employee.EmployeeCode}");

                // Tạo object để gửi API
                var employeeData = new
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.EmployeeName,
                    DepartmentCode = employee.DepartmentCode,
                    Email = employee.Email,
                };

                // Serialize object thành JSON
                var json = JsonSerializer.Serialize(employeeData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation($"Gửi yêu cầu đến: {_apiBaseUrl}/Add");
                _logger.LogInformation($"Dữ liệu gửi lên Server: {json}");

                // Gọi API Add
                var reponse = await _httpClient.PostAsync($"{_apiBaseUrl}/Add", content);
                var reponseContent = await reponse.Content.ReadAsStringAsync();

                _logger.LogInformation($"Trạng thái phản hồi từ API: {reponse.StatusCode}");
                _logger.LogInformation($"Nội dung trả về từ API: {reponseContent}");

                if (reponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Đã thêm nhân sự thành công với mã: {employee.EmployeeCode}");
                    return new ApiResponse
                    {
                        Success = true,
                        Message = "Thêm nhân sự thành công",
                        Data = employeeData,
                    };
                }
                else if (reponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Trường hợp mã nhân viên đã tồn tại
                    _logger.LogWarning($"Mã nhân sự {employee.EmployeeCode} đã tồn tại");
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Mã nhân sự {employee.EmployeeCode} đã tồn tại"
                    };
                }
                else if (reponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Trường hợp dữ liệu không hợp lệ
                    _logger.LogError($"Yêu cầu không hợp lệ với nhân sự: {employee.EmployeeCode}. Phản hồi: {reponseContent}");
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Thông tin nhân sự không hợp lệ"
                    };
                }
                else
                {
                    // Các lỗi khác
                    _logger.LogError($"AddEmployeeAsync không thành công với trạng thái: {reponse.StatusCode}. Phản hồi: {reponseContent}");
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Có lỗi xảy ra khi thêm nhân viên. Mã lỗi {reponse.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, $"Lỗi HTTP trong AddEmployeeAsync cho nhân sự: {employee.EmployeeCode}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không thể kết nối server. Vui lòng kiểm tra lại."
                };
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LogError(tcEx, $"Yêu cầu thời gian chờ trong AddEmployeeAsync cho nhân sự: {employee.EmployeeCode}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Yêu cầu bị timeout. Vui lòng thử lại."
                };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON serialization error in AddEmployeeAsync for employee: {employee.EmployeeCode}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Lỗi xử lý dữ liệu. Vui lòng thử lại."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in AddEmployeeAsync for employee: {employee.EmployeeCode}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Có lỗi không mong muốn xảy ra. Vui lòng liên hệ quản trị viên."
                };
            }
        }

        // Lấy tất cả nhân sự
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/GetAll");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return employees ?? new List<Employee>();
                }
                else
                {
                    _logger.LogError($"GetAllEmployeesAsync failed with status: {response.StatusCode}");
                }

                return new List<Employee>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllEmployeesAsync");
                return new List<Employee>();
            }
        }

        // Lấy nhân sự theo id
        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employee = JsonSerializer.Deserialize<Employee>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return employee;
                }
                else
                {
                    _logger.LogError($"GetEmployeeByIdAsync failed with status: {response.StatusCode} for ID: {id}");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetEmployeeByIdAsync for ID: {id}");
                return null;
            }
        }

        // Lấy nhân sự theo mã bộ phận
        public async Task<List<Employee>> SearchByDepartmentCodeAsync(string departmentCode)
        {
            try
            {
                var url = $"{_apiBaseUrl}/ByDepartment/{Uri.EscapeDataString(departmentCode)}";
                _logger.LogInformation($"Calling API: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return employees ?? new List<Employee>();
                }
                else
                {
                    _logger.LogError($"SearchByDepartmentCodeAsync failed with status: {response.StatusCode} for department: {departmentCode}");
                }

                return new List<Employee>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchByDepartmentCodeAsync for department: {departmentCode}");
                return new List<Employee>();
            }
        }

        // Lấy nhân sự theo mã nhân sự - KIỂM TRA LẠI ENDPOINT
        public async Task<List<Employee>> SearchByEmployeeCodeAsync(string employeeCode)
        {
            try
            {
                // Thử các endpoint khác nhau - tùy thuộc vào API backend của bạn
                var endpoints = new[]
                {
                    $"{_apiBaseUrl}/GetEmployeeCode/{Uri.EscapeDataString(employeeCode)}",
                    $"{_apiBaseUrl}/ByEmployeeCode/{Uri.EscapeDataString(employeeCode)}",
                    $"{_apiBaseUrl}/SearchByCode/{Uri.EscapeDataString(employeeCode)}",
                    $"{_apiBaseUrl}/Employee/Code/{Uri.EscapeDataString(employeeCode)}"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        _logger.LogInformation($"Trying API endpoint: {endpoint}");
                        var response = await _httpClient.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"API Response: {json}");

                            var employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (employees != null && employees.Any())
                            {
                                _logger.LogInformation($"Found {employees.Count} employees with endpoint: {endpoint}");
                                return employees;
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Endpoint {endpoint} failed with status: {response.StatusCode}");
                        }
                    }
                    catch (Exception endpointEx)
                    {
                        _logger.LogWarning(endpointEx, $"Endpoint {endpoint} threw exception");
                        continue;
                    }
                }

                _logger.LogError($"All endpoints failed for employee code: {employeeCode}");
                return new List<Employee>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchByEmployeeCodeAsync for employee code: {employeeCode}");
                return new List<Employee>();
            }
        }

        // Lấy nhân sự theo tên - KIỂM TRA LẠI ENDPOINT
        public async Task<List<Employee>> SearchByNameAsync(string name)
        {
            try
            {
                // Thử các endpoint khác nhau - tùy thuộc vào API backend của bạn
                var endpoints = new[]
                {
                    $"{_apiBaseUrl}/GetEmployeeName/{Uri.EscapeDataString(name)}",
                    $"{_apiBaseUrl}/ByName/{Uri.EscapeDataString(name)}",
                    $"{_apiBaseUrl}/SearchByName/{Uri.EscapeDataString(name)}",
                    $"{_apiBaseUrl}/Employee/Name/{Uri.EscapeDataString(name)}"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        _logger.LogInformation($"Trying API endpoint: {endpoint}");
                        var response = await _httpClient.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"API Response: {json}");

                            var employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (employees != null && employees.Any())
                            {
                                _logger.LogInformation($"Found {employees.Count} employees with endpoint: {endpoint}");
                                return employees;
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Endpoint {endpoint} failed with status: {response.StatusCode}");
                        }
                    }
                    catch (Exception endpointEx)
                    {
                        _logger.LogWarning(endpointEx, $"Endpoint {endpoint} threw exception");
                        continue;
                    }
                }

                _logger.LogError($"All endpoints failed for employee name: {name}");
                return new List<Employee>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchByNameAsync for name: {name}");
                return new List<Employee>();
            }
        }

        public async Task<bool> UpdateEmployeeAsync(string employeeCode, Employee employee)
        {
            try
            {
                var employeeDto = new
                {
                    EmployeeName = employee.employeeName,
                    DepartmentCode = employee.departmentCode,
                    Email = employee.email
                };

                var json = JsonSerializer.Serialize(employeeDto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/Update/{Uri.EscapeDataString(employeeCode)}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully updated employee with code: {employeeCode}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"UpdateEmployeeAsync failed with status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateEmployeeAsync for employee code: {employeeCode}");
                return false;
            }
        }
    }
}