namespace WebPhongKham.Models
{
    public class TaiKhoan
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";
        public string VaiTro { get; set; } = "";  

        public int? IdBenhNhan { get; set; }
        public int? IdBacSi { get; set; }
                public int TrangThai { get; set; } = 1;
    }
}
