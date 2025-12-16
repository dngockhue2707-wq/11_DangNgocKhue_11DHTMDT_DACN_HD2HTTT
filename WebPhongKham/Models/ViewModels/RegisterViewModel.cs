namespace WebPhongKham.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string HoTen { get; set; } = "";
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string SDT { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";
        public string? Message { get; set; }
    }
}
