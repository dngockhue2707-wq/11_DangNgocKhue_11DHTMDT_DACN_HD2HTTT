namespace WebPhongKham.Models
{
    public class BacSi_LichLamViec
    {
        public int Id { get; set; }
        public int IdBacSi { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public int ThuTrongTuan { get; set; }
    }
}
