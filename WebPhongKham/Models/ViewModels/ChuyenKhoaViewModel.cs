using System.Collections.Generic;

namespace WebPhongKham.Models.ViewModels
{
    public class ChuyenKhoaViewModel
    {
        public string? SelectedChuyenKhoa { get; set; }
        public List<string> ChuyenKhoas { get; set; } = new();
        public List<BacSi> BacSis { get; set; } = new();
    }
}
