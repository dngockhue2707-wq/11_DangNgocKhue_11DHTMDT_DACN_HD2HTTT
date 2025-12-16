namespace WebPhongKham.Models
{
    public class ThongBao
    {
        public int IdThongBao { get; set; }
        public int IdBenhNhan { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public bool DaDoc { get; set; }
        public DateTime? NgayGui { get; set; }
        public bool DaXem { get; set; }
    }
}
