using System.ComponentModel.DataAnnotations.Schema;
    namespace WebPhongKham.Models
    {
        public class LichHen
        {
            public int IdLichHen { get; set; }
            public int IdBenhNhan { get; set; }
            public int IdBacSi { get; set; }
            [NotMapped]
            public string TenBenhNhan { get; set; } = "";
            [NotMapped]
            public string TenBacSi { get; set; } = "";
            public DateTime Ngay { get; set; }
            public TimeSpan Gio { get; set; } 
            public string TrangThai { get; set; } = "";
            public DateTime NgayTao { get; set; }
            public DateTime? NgayHuy { get; set; }
            public string? LyDoHuy { get; set; }
            public string? NguoiHuy { get; set; }
            public string? PhieuKham { get; set; }
            public string? LyDoKham { get; set; }
        }
    }
