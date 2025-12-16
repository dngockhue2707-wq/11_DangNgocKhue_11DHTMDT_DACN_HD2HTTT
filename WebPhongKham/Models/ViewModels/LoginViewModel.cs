namespace WebPhongKham.Models.ViewModels
{
    public class LoginViewModel
    {
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";

        public string? Message { get; set; }
        public string? VaiTro { get; set; }
        public int? IdBenhNhan { get; set; }
        public int? IdBacSi { get; set; }
    }
}
