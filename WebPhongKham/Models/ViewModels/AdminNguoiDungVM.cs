namespace WebPhongKham.Models.ViewModels
{
    public class AdminNguoiDungVM
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? VaiTro { get; set; }
        public int TrangThai { get; set; }
        public string? HoTen { get; set; }
        public int IdBenhNhan { get; set; }
        public int IdBacSi { get; set; }
    }
}
