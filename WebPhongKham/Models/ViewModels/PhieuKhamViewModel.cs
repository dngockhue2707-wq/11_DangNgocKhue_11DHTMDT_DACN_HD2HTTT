namespace WebPhongKham.Models.ViewModels
{
    public class PhieuKhamViewModel
    {
        public int IdLichHen { get; set; }
        public string? PhieuKham { get; set; }
        public string? HoTen { get; set; }
        public string? TenBacSi { get; set; }
        public DateTime NgayKham { get; set; }
        public TimeSpan GioKham { get; set; }
        public DateTime NgayDat { get; set; }
        public string? LyDoKham { get; set; }
        public string? TrangThai { get; set; }
    }
}
