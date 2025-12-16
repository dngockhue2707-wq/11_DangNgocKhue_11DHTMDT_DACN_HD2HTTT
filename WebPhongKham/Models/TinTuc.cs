using System;

namespace WebPhongKham.Models
{
    public class TinTuc
    {
        public int Id { get; set; }
        public string? TieuDe { get; set; }
        public string? MoTaNgan { get; set; }
        public string? LoaiTin { get; set; }
        public DateTime NgayDang { get; set; }
        public string? NoiDung { get; set; }
        public bool TrangThai { get; set; }
    }
}
