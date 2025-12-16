namespace WebPhongKham.Models
{
    public class BenhNhan
    {
        public int IdBenhNhan { get; set; }
        public string HoTen { get; set; } = "";
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string SDT { get; set; } = "";
        public string Email { get; set; } = "";
        public bool CoTaiKhoan { get; set; }   // 0: vãng lai, 1: có tài khoản
    }
}
