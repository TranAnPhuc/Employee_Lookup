using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Employee_Lookup.Models
{
    public class SearchViewModel
    {
        [Display(Name = "Tên nhân sự")]
        public string employeeName { get; set; }

        [Display(Name = "Mã nhân sự")]
        public string employeeCode { get; set; }

        [Display(Name = "Mã bộ phận")]
        public string departmentCode { get; set; }

        public List<Employee> SearchResults { get; set; } = new List<Employee>();
        public int TotalRecords { get; set; }
        public string SearchType { get; set; }

        // Properties cho sorting
        public string SortBy { get; set; } = "employeeName"; // Mặc định sắp xếp theo tên

        public string SortDirection { get; set; } = "asc"; // Mặc định tăng dần

        // Properties cho pagination
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        // Helper properties cho pagination
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartRecord => TotalRecords == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int EndRecord => Math.Min(PageNumber * PageSize, TotalRecords);

        // Method để reset về sắp xếp mặc định
        public void ResetSort()
        {
            SortBy = "employeeName";
            SortDirection = "asc";
        }

        // Method để reset về trang đầu
        public void ResetPaging()
        {
            PageNumber = 1;
        }

        // Method để validate page number
        public void ValidatePageNumber()
        {
            if (PageNumber < 1)
                PageNumber = 1;
            else if (PageNumber > TotalPages && TotalPages > 0)
                PageNumber = TotalPages;
        }

        // Method để lấy danh sách các trang hiển thị trong pagination
        public List<int> GetPageNumbers()
        {
            var pages = new List<int>();
            var startPage = Math.Max(1, PageNumber - 2);
            var endPage = Math.Min(TotalPages, PageNumber + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages;
        }
    }
}