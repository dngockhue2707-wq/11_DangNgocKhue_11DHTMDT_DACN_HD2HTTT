using System;
using System.Collections.Generic;

namespace WebPhongKham.Models.ViewModels
{
    public class DatLichViewModel
    {
        public int? IdBenhNhan { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }   
        public string? GioiTinh { get; set; } 
        public string? DiaChi { get; set; }  
        public string? SDT { get; set; }
        public string? Email { get; set; }

        public int? IdBacSi { get; set; }
        public DateTime? Ngay { get; set; }
        public string? Gio { get; set; }
        public string? LyDoKham { get; set; }

        public List<BacSi> DanhSachBacSi { get; set; } = new();
        public List<string> GioList { get; set; } = new();   
        public string? TenBacSi { get; set; }
    }
}
