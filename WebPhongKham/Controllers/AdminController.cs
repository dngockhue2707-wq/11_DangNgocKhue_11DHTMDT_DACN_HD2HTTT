using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class AdminController : Controller
    {
        private readonly ClinicDbContext _context;

        public AdminController(ClinicDbContext context)
        {
            _context = context;
        }

        private bool CheckAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        public async Task<IActionResult> Dashboard(string period = "month", int? year = null, int? month = null)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var now = DateTime.Now;
            var today = now.Date;

            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;

            ViewBag.Period = period;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;

            ViewBag.TongBenhNhan = await _context.BenhNhans.CountAsync();
            ViewBag.TongBacSi = await _context.BacSis.CountAsync();
            ViewBag.TongLich = await _context.LichHens.CountAsync();

            var top = await (from lh in _context.LichHens.AsNoTracking()
                             join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                             group new { lh, bs } by new { lh.IdBacSi, bs.HoTen } into g
                             orderby g.Count() descending
                             select new
                             {
                                 HoTen = g.Key.HoTen,
                                 SoLich = g.Count()
                             }).FirstOrDefaultAsync();

            if (top != null)
            {
                ViewBag.TopBacSi = top.HoTen;
                ViewBag.TopSoLich = top.SoLich;
            }
            else
            {
                ViewBag.TopBacSi = "Chưa có dữ liệu";
                ViewBag.TopSoLich = 0;
            }

            ViewBag.TongHomNay = await _context.LichHens.AsNoTracking()
                .CountAsync(x => x.Ngay.Date == today);

            ViewBag.TongDangChoHomNay = await _context.LichHens.AsNoTracking()
                .CountAsync(x => x.Ngay.Date == today && x.TrangThai == "Đã đặt");

            ViewBag.TongDaHuyHomNay = await _context.LichHens.AsNoTracking()
                .CountAsync(x => x.Ngay.Date == today && x.TrangThai == "Đã hủy");

            var yearList = await _context.LichHens.AsNoTracking()
                .Select(x => x.Ngay.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();
            if (yearList.Count == 0) yearList.Add(now.Year);
            ViewBag.YearList = yearList;

            if (period == "week")
            {
                var start = StartOfWeek(today, DayOfWeek.Monday);
                var end = start.AddDays(7);

                ViewBag.TongTuanNay = await _context.LichHens.AsNoTracking()
                    .CountAsync(x => x.Ngay >= start && x.Ngay < end);

                var rows = await _context.LichHens.AsNoTracking()
                    .Where(x => x.Ngay >= start && x.Ngay < end)
                    .GroupBy(x => x.Ngay.Date)
                    .Select(g => new { Day = g.Key, SoLich = g.Count() })
                    .ToListAsync();

                var labels = new List<string>();
                var values = new List<int>();

                for (int i = 0; i < 7; i++)
                {
                    var d = start.AddDays(i);
                    labels.Add(d.ToString("dd/MM"));
                    values.Add(rows.FirstOrDefault(r => r.Day == d)?.SoLich ?? 0);
                }

                ViewBag.ChartTitle = "Số lịch hẹn theo ngày (tuần này)";
                ViewBag.ChartLabels = labels;
                ViewBag.ChartValues = values;

                return View();
            }

            if (period == "year")
            {
                var start = new DateTime(selectedYear, 1, 1);
                var end = start.AddYears(1);

                ViewBag.TongNam = await _context.LichHens.AsNoTracking()
                    .CountAsync(x => x.Ngay >= start && x.Ngay < end);

                var rows = await _context.LichHens.AsNoTracking()
                    .Where(x => x.Ngay >= start && x.Ngay < end)
                    .GroupBy(x => x.Ngay.Month)
                    .Select(g => new { Month = g.Key, SoLich = g.Count() })
                    .ToListAsync();

                var labels = new List<string>();
                var values = new List<int>();

                for (int m = 1; m <= 12; m++)
                {
                    labels.Add($"T{m:00}");
                    values.Add(rows.FirstOrDefault(r => r.Month == m)?.SoLich ?? 0);
                }

                ViewBag.ChartTitle = $"Số lịch hẹn theo tháng ({selectedYear})";
                ViewBag.ChartLabels = labels;
                ViewBag.ChartValues = values;

                return View();
            }

            {
                var start = new DateTime(selectedYear, selectedMonth, 1);
                var end = start.AddMonths(1);

                ViewBag.TongThang = await _context.LichHens.AsNoTracking()
                    .CountAsync(x => x.Ngay >= start && x.Ngay < end);

                var rows = await _context.LichHens.AsNoTracking()
                    .Where(x => x.Ngay >= start && x.Ngay < end)
                    .GroupBy(x => x.Ngay.Date)
                    .Select(g => new { Day = g.Key, SoLich = g.Count() })
                    .ToListAsync();

                int days = DateTime.DaysInMonth(selectedYear, selectedMonth);
                var labels = new List<string>();
                var values = new List<int>();

                for (int i = 1; i <= days; i++)
                {
                    var d = new DateTime(selectedYear, selectedMonth, i);
                    labels.Add(d.ToString("dd/MM"));
                    values.Add(rows.FirstOrDefault(r => r.Day == d)?.SoLich ?? 0);
                }

                ViewBag.ChartTitle = $"Số lịch hẹn theo ngày ({selectedMonth:00}/{selectedYear})";
                ViewBag.ChartLabels = labels;
                ViewBag.ChartValues = values;
            }

            return View();
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-diff).Date;
        }

        public async Task<IActionResult> BacSi()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var list = await _context.BacSis.AsNoTracking()
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ThemBacSi(BacSi model)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                if (string.IsNullOrWhiteSpace(model.HoTen) || string.IsNullOrWhiteSpace(model.ChuyenKhoa))
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ Họ tên và Chuyên khoa.";
                    return RedirectToAction("BacSi");
                }

                _context.BacSis.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm bác sĩ thành công.";
                return RedirectToAction("BacSi");
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? "";
                if (msg.Contains("OUTPUT clause", StringComparison.OrdinalIgnoreCase))
                    TempData["Error"] = "Không thể thêm bác sĩ do bảng BacSi đang có trigger. Hãy cấu hình DbContext/trigger đúng cách.";
                else
                    TempData["Error"] = "Không thể thêm bác sĩ. Vui lòng thử lại.";

                return RedirectToAction("BacSi");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SuaBacSi(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var bs = await _context.BacSis.FindAsync(id);
            if (bs == null) return NotFound();

            return View(bs);
        }

        [HttpPost]
        public async Task<IActionResult> SuaBacSi(BacSi model)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var bs = await _context.BacSis.FindAsync(model.IdBacSi);
                if (bs == null)
                {
                    TempData["Error"] = "Không tìm thấy bác sĩ.";
                    return RedirectToAction("BacSi");
                }

                bs.HoTen = model.HoTen;
                bs.ChuyenKhoa = model.ChuyenKhoa;
                bs.KinhNghiem = model.KinhNghiem;
                bs.MoTa = model.MoTa;
                bs.HinhAnh = model.HinhAnh;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật bác sĩ thành công.";
                return RedirectToAction("BacSi");
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể cập nhật bác sĩ. Vui lòng thử lại.";
                return RedirectToAction("BacSi");
            }
        }

        public async Task<IActionResult> DanhSachLich()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var list = await (from lh in _context.LichHens.AsNoTracking()
                              join bn in _context.BenhNhans.AsNoTracking() on lh.IdBenhNhan equals bn.IdBenhNhan
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              orderby lh.Ngay descending, lh.Gio descending
                              select new LichHen
                              {
                                  IdLichHen = lh.IdLichHen,
                                  IdBenhNhan = lh.IdBenhNhan,
                                  IdBacSi = lh.IdBacSi,
                                  TenBenhNhan = bn.HoTen,
                                  TenBacSi = bs != null ? (bs.HoTen ?? "") : "",
                                  Ngay = lh.Ngay,
                                  Gio = lh.Gio,
                                  TrangThai = lh.TrangThai,
                                  NgayTao = lh.NgayTao,
                                  NgayHuy = lh.NgayHuy,
                                  LyDoHuy = lh.LyDoHuy,
                                  NguoiHuy = lh.NguoiHuy,
                                  LyDoKham = lh.LyDoKham
                              }).ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> NgayNghi(int idBacSi)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.IdBacSi = idBacSi;

            var list = await _context.BacSi_NgayNghis.AsNoTracking()
                .Where(x => x.IdBacSi == idBacSi)
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ThemNgayNghi(int idBacSi, DateTime ngay, string ghichu)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var existed = await _context.BacSi_NgayNghis.AsNoTracking()
                    .AnyAsync(x => x.IdBacSi == idBacSi && x.Ngay.Date == ngay.Date);

                if (existed)
                {
                    TempData["Error"] = "Ngày nghỉ này đã tồn tại.";
                    return RedirectToAction("NgayNghi", new { idBacSi });
                }

                _context.BacSi_NgayNghis.Add(new BacSi_NgayNghi
                {
                    IdBacSi = idBacSi,
                    Ngay = ngay.Date,
                    GhiChu = ghichu ?? ""
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã thêm ngày nghỉ.";
                return RedirectToAction("NgayNghi", new { idBacSi });
            }
            catch
            {
                TempData["Error"] = "Không thể thêm ngày nghỉ.";
                return RedirectToAction("NgayNghi", new { idBacSi });
            }
        }

        [HttpPost]
        public async Task<IActionResult> XoaNgayNghi(int id, int idBacSi)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var item = await _context.BacSi_NgayNghis.FindAsync(id);
                if (item != null)
                {
                    _context.BacSi_NgayNghis.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa ngày nghỉ.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy ngày nghỉ.";
                }

                return RedirectToAction("NgayNghi", new { idBacSi });
            }
            catch
            {
                TempData["Error"] = "Không thể xóa ngày nghỉ.";
                return RedirectToAction("NgayNghi", new { idBacSi });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LichLamViec(int idBacSi)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.IdBacSi = idBacSi;

            var list = await _context.BacSi_LichLamViecs.AsNoTracking()
                .Where(x => x.IdBacSi == idBacSi)
                .OrderBy(x => x.ThuTrongTuan)
                .ThenBy(x => x.GioBatDau)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ThemLichLamViec(int idBacSi, int thu, string gbd, string gkt)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                if (!TimeSpan.TryParse(gbd, out var gioBatDau) || !TimeSpan.TryParse(gkt, out var gioKetThuc))
                {
                    TempData["Error"] = "Giờ bắt đầu/kết thúc không hợp lệ.";
                    return RedirectToAction("LichLamViec", new { idBacSi });
                }

                if (gioBatDau >= gioKetThuc)
                {
                    TempData["Error"] = "Giờ bắt đầu phải nhỏ hơn giờ kết thúc.";
                    return RedirectToAction("LichLamViec", new { idBacSi });
                }

                var existed = await _context.BacSi_LichLamViecs.AsNoTracking()
                    .AnyAsync(x => x.IdBacSi == idBacSi && x.ThuTrongTuan == thu && x.GioBatDau == gioBatDau && x.GioKetThuc == gioKetThuc);

                if (existed)
                {
                    TempData["Error"] = "Ca làm việc này đã tồn tại.";
                    return RedirectToAction("LichLamViec", new { idBacSi });
                }

                _context.BacSi_LichLamViecs.Add(new BacSi_LichLamViec
                {
                    IdBacSi = idBacSi,
                    ThuTrongTuan = thu,
                    GioBatDau = gioBatDau,
                    GioKetThuc = gioKetThuc
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã thêm lịch làm việc.";
                return RedirectToAction("LichLamViec", new { idBacSi });
            }
            catch
            {
                TempData["Error"] = "Không thể thêm lịch làm việc.";
                return RedirectToAction("LichLamViec", new { idBacSi });
            }
        }

        [HttpPost]
        public async Task<IActionResult> XoaLichLamViec(int id, int idBacSi)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var item = await _context.BacSi_LichLamViecs.FindAsync(id);
                if (item != null)
                {
                    _context.BacSi_LichLamViecs.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa lịch làm việc.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy lịch làm việc.";
                }

                return RedirectToAction("LichLamViec", new { idBacSi });
            }
            catch
            {
                TempData["Error"] = "Không thể xóa lịch làm việc.";
                return RedirectToAction("LichLamViec", new { idBacSi });
            }
        }

        public async Task<IActionResult> NguoiDung()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var list = await (
                from tk in _context.TaiKhoans.AsNoTracking()
                join bs in _context.BacSis.AsNoTracking()
                    on tk.IdBacSi equals bs.IdBacSi into bsj
                from bs in bsj.DefaultIfEmpty()
                join bn in _context.BenhNhans.AsNoTracking()
                    on tk.IdBenhNhan equals bn.IdBenhNhan into bnj
                from bn in bnj.DefaultIfEmpty()
                select new AdminNguoiDungVM
                {
                    Id = tk.Id,
                    Email = tk.Email,
                    VaiTro = tk.VaiTro,
                    TrangThai = tk.TrangThai,
                    HoTen = bs != null ? (bs.HoTen ?? "")
                        : bn != null ? (bn.HoTen ?? "")
                        : "Chưa gán"
                }
            ).OrderByDescending(x => x.Id).ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThaiNguoiDung(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");

            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId.HasValue && id == myId.Value)
            {
                TempData["Error"] = "Không thể tự khóa tài khoản đang đăng nhập.";
                return RedirectToAction("NguoiDung");
            }

            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("NguoiDung");
            }

            if (tk.VaiTro == "Admin")
            {
                TempData["Error"] = "Không được khóa tài khoản Admin.";
                return RedirectToAction("NguoiDung");
            }

            tk.TrangThai = tk.TrangThai == 1 ? 0 : 1;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật trạng thái người dùng.";
            return RedirectToAction("NguoiDung");
        }
    }
}
