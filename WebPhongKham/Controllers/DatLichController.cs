using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class DatLichController : Controller
    {
        private readonly ClinicDbContext _context;

        public DatLichController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? bacSiId)
        {
            var vm = new DatLichViewModel
            {
                Ngay = DateTime.Today,
                DanhSachBacSi = new List<BacSi>(),
                GioList = new List<string>(),
                IdBacSi = bacSiId ?? 0
            };

            vm.DanhSachBacSi = await _context.BacSis
                .AsNoTracking()
                .Select(x => new BacSi
                {
                    IdBacSi = x.IdBacSi,
                    HoTen = x.HoTen,
                    ChuyenKhoa = x.ChuyenKhoa
                })
                .ToListAsync();

            if (HttpContext.Session.GetString("VaiTro") == "User")
            {
                var idBenhNhan = HttpContext.Session.GetInt32("IdBenhNhan");
                if (idBenhNhan != null)
                {
                    var bn = await _context.BenhNhans.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.IdBenhNhan == idBenhNhan.Value);

                    if (bn != null)
                    {
                        vm.HoTen = bn.HoTen;
                        vm.NgaySinh = bn.NgaySinh;
                        vm.GioiTinh = bn.GioiTinh ?? "";
                        vm.SDT = bn.SDT;
                        vm.Email = bn.Email ?? "";
                        vm.DiaChi = bn.DiaChi ?? "";
                    }
                }
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> LayGioTheoBacSi(int idBacSi, DateTime ngay)
        {
            var gioList = new List<string>();

            var thu = (int)ngay.DayOfWeek + 1;

            var ca = await _context.BacSi_LichLamViecs
                .AsNoTracking()
                .Where(x => x.IdBacSi == idBacSi && x.ThuTrongTuan == thu)
                .OrderBy(x => x.GioBatDau)
                .FirstOrDefaultAsync();

            if (ca != null)
            {
                var gio = ca.GioBatDau;
                while (gio < ca.GioKetThuc)
                {
                    gioList.Add(gio.ToString(@"hh\:mm"));
                    gio = gio.Add(TimeSpan.FromMinutes(30));
                }
            }

            return Json(gioList);
        }

        [HttpPost]
        public async Task<IActionResult> Index(DatLichViewModel model)
        {
            int idBenhNhan;

            if (HttpContext.Session.GetString("VaiTro") == "User")
            {
                idBenhNhan = HttpContext.Session.GetInt32("IdBenhNhan") ?? 0;
            }
            else
            {
                var bnExist = await _context.BenhNhans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SDT == (model.SDT ?? ""));

                if (bnExist != null)
                {
                    idBenhNhan = bnExist.IdBenhNhan;
                }
                else
                {
                    var bnNew = new BenhNhan
                    {
                        HoTen = model.HoTen ?? "",
                        NgaySinh = model.NgaySinh,
                        GioiTinh = model.GioiTinh,
                        SDT = model.SDT ?? "",
                        Email = model.Email ?? "",
                        DiaChi = model.DiaChi ?? ""
                    };

                    _context.BenhNhans.Add(bnNew);
                    await _context.SaveChangesAsync();

                    idBenhNhan = bnNew.IdBenhNhan;
                }
            }

            if (!model.IdBacSi.HasValue || !model.Ngay.HasValue || string.IsNullOrWhiteSpace(model.Gio))
                return RedirectToAction("Index");

            if (!TimeSpan.TryParse(model.Gio, out var gio))
                return RedirectToAction("Index");

            var lh = new LichHen
            {
                IdBenhNhan = idBenhNhan,
                IdBacSi = model.IdBacSi.Value,
                Ngay = model.Ngay.Value,
                Gio = gio,
                TrangThai = "Chờ duyệt",
                NgayTao = DateTime.Now,
                LyDoKham = model.LyDoKham ?? ""
            };

            try
            {
                _context.LichHens.Add(lh);
                await _context.SaveChangesAsync();

                return RedirectToAction("PhieuKham", new { id = lh.IdLichHen });
            }
            catch (DbUpdateException ex)
            {
                var baseEx = ex.GetBaseException();

                if (baseEx is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    {
                        ViewBag.Error = "Khung giờ này đã có người đặt hoặc bị trùng lịch. Vui lòng chọn giờ khác.";
                    }
                    else if (sqlEx.Number >= 50000)
                    {
                        if (sqlEx.Number == 50001)
                            ViewBag.Error = "Giờ này bác sĩ không làm việc. Vui lòng chọn giờ khác.";
                        else if (sqlEx.Number == 50002)
                            ViewBag.Error = "Bác sĩ không làm việc vào ngày này. Vui lòng chọn ngày khác.";
                        else if (sqlEx.Number == 50003)
                            ViewBag.Error = "Bạn đã có lịch hẹn trùng khung giờ này. Vui lòng chọn giờ khác.";
                        else
                            ViewBag.Error = "Không thể đặt lịch: " + sqlEx.Message;
                    }
                    else
                    {
                        ViewBag.Error = "Không thể đặt lịch. Vui lòng kiểm tra lại thông tin và thử lại.";
                    }
                }
                else
                {
                    var msg = (baseEx.Message ?? "").ToLowerInvariant();

                    if (msg.Contains("giờ này bác sĩ không làm việc"))
                        ViewBag.Error = "Giờ này bác sĩ không làm việc. Vui lòng chọn giờ khác.";
                    else if (msg.Contains("ngày nghỉ") || msg.Contains("bác sĩ nghỉ"))
                        ViewBag.Error = "Bác sĩ không làm việc vào ngày này. Vui lòng chọn ngày khác.";
                    else if (msg.Contains("duplicate key") || msg.Contains("cannot insert duplicate") || msg.Contains("trùng") || msg.Contains("đã được đặt") || msg.Contains("đã có lịch"))
                        ViewBag.Error = "Khung giờ này đã có người đặt hoặc bị trùng lịch. Vui lòng chọn giờ khác.";
                    else if (msg.Contains("bạn đã có lịch") || msg.Contains("đã đặt lịch"))
                        ViewBag.Error = "Bạn đã có lịch hẹn trùng khung giờ này. Vui lòng chọn giờ khác.";
                    else
                        ViewBag.Error = "Không thể đặt lịch. Vui lòng kiểm tra lại thông tin và thử lại.";
                }

                var vm = new DatLichViewModel
                {
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    GioiTinh = model.GioiTinh,
                    SDT = model.SDT,
                    Email = model.Email,
                    DiaChi = model.DiaChi,
                    Ngay = model.Ngay,
                    Gio = model.Gio,
                    LyDoKham = model.LyDoKham,
                    IdBacSi = model.IdBacSi ?? 0,
                    DanhSachBacSi = await _context.BacSis
                        .AsNoTracking()
                        .Select(x => new BacSi { IdBacSi = x.IdBacSi, HoTen = x.HoTen, ChuyenKhoa = x.ChuyenKhoa })
                        .ToListAsync(),
                    GioList = new List<string>()
                };

                return View(vm);
            }
        }

        public async Task<IActionResult> PhieuKham(int id)
        {
            var data = await (from lh in _context.LichHens.AsNoTracking()
                              join bn in _context.BenhNhans.AsNoTracking() on lh.IdBenhNhan equals bn.IdBenhNhan
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              where lh.IdLichHen == id
                              select new
                              {
                                  PhieuKham = EF.Property<string>(lh, "PhieuKham"),
                                  HoTen = bn.HoTen,
                                  TenBacSi = bs.HoTen,
                                  Ngay = lh.Ngay,
                                  Gio = lh.Gio,
                                  NgayTao = lh.NgayTao,
                                  LyDoKham = lh.LyDoKham,
                                  TrangThai = lh.TrangThai
                              }).FirstOrDefaultAsync();

            if (data == null) return NotFound();

            var vm = new PhieuKhamViewModel
            {
                PhieuKham = data.PhieuKham ?? "",
                HoTen = data.HoTen,
                TenBacSi = data.TenBacSi,
                NgayKham = data.Ngay,
                GioKham = data.Gio,
                NgayDat = data.NgayTao,
                LyDoKham = data.LyDoKham ?? "",
                TrangThai = data.TrangThai
            };

            return View(vm);
        }

        public async Task<IActionResult> LichSu()
        {
            var idBenhNhan = HttpContext.Session.GetInt32("IdBenhNhan");
            if (idBenhNhan == null)
                return RedirectToAction("Login", "Account");

            var rows = await (from lh in _context.LichHens.AsNoTracking()
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              where lh.IdBenhNhan == idBenhNhan.Value
                              orderby lh.Ngay descending
                              select new
                              {
                                  PhieuKham = EF.Property<string>(lh, "PhieuKham"),
                                  TenBacSi = bs.HoTen,
                                  Ngay = lh.Ngay,
                                  Gio = lh.Gio,
                                  TrangThai = lh.TrangThai
                              }).ToListAsync();

            var list = rows.Select(x => new PhieuKhamViewModel
            {
                PhieuKham = x.PhieuKham ?? "",
                TenBacSi = x.TenBacSi,
                NgayKham = x.Ngay,
                GioKham = x.Gio,
                TrangThai = x.TrangThai
            }).ToList();

            return View(list);
        }

        public async Task<IActionResult> PhieuKhamByMa(string ma)
        {
            var data = await (from lh in _context.LichHens.AsNoTracking()
                              join bn in _context.BenhNhans.AsNoTracking() on lh.IdBenhNhan equals bn.IdBenhNhan
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              where EF.Property<string>(lh, "PhieuKham") == ma
                              select new
                              {
                                  PhieuKham = EF.Property<string>(lh, "PhieuKham"),
                                  HoTen = bn.HoTen,
                                  TenBacSi = bs.HoTen,
                                  Ngay = lh.Ngay,
                                  Gio = lh.Gio,
                                  NgayTao = lh.NgayTao,
                                  LyDoKham = lh.LyDoKham,
                                  TrangThai = lh.TrangThai
                              }).FirstOrDefaultAsync();

            if (data == null) return Content("Không tìm thấy phiếu khám");

            var vm = new PhieuKhamViewModel
            {
                PhieuKham = data.PhieuKham ?? "",
                HoTen = data.HoTen,
                TenBacSi = data.TenBacSi,
                NgayKham = data.Ngay,
                GioKham = data.Gio,
                NgayDat = data.NgayTao,
                LyDoKham = data.LyDoKham ?? "",
                TrangThai = data.TrangThai
            };

            return View("PhieuKham", vm);
        }
    }
}
