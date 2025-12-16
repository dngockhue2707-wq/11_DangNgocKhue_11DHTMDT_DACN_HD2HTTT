public class SMS_Log
{
    public int Id { get; set; }
    public string? SoDienThoai { get; set; }
    public string? NoiDung { get; set; }
    public DateTime NgayGui { get; set; }
    public string? TrangThai { get; set; }

    public int? IdLichHen { get; set; }
    public string? LoaiSMS { get; set; }
}
