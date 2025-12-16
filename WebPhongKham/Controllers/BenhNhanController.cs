using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class BenhNhanController : Controller
    {
        private readonly ClinicDbContext _context;

        public BenhNhanController(ClinicDbContext context)
        {
            _context = context;
        }

        private async Task DemThongBao()
        {
            var id = HttpContext.Session.GetInt32("IdBenhNhan");
            if (id == null) return;

            ViewBag.SoThongBaoChuaDoc = await _context.ThongBaos
                .AsNoTracking()
                .CountAsync(x => x.IdBenhNhan == id.Value && x.DaDoc == false);
        }

        [HttpGet]
        public async Task<IActionResult> TraCuu(string? sdt)
        {
            await DemThongBao();

            var list = new List<PhieuKhamViewModel>();

            if (string.IsNullOrWhiteSpace(sdt))
            {
                ViewBag.SDT = "";
                return View(list);
            }

            var data = await (from lh in _context.LichHens.AsNoTracking()
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              join bn in _context.BenhNhans.AsNoTracking() on lh.IdBenhNhan equals bn.IdBenhNhan
                              where bn.SDT == sdt
                              orderby lh.Ngay descending, lh.Gio descending
                              select new
                              {
                                  lh.IdLichHen,
                                  lh.PhieuKham,
                                  TenBacSi = bs.HoTen,
                                  lh.Ngay,
                                  lh.Gio,
                                  lh.TrangThai
                              }).ToListAsync();

            foreach (var x in data)
            {
                list.Add(new PhieuKhamViewModel
                {
                    IdLichHen = x.IdLichHen,
                    PhieuKham = x.PhieuKham ?? "",
                    TenBacSi = x.TenBacSi,
                    NgayKham = x.Ngay,
                    GioKham = x.Gio,
                    TrangThai = x.TrangThai
                });
            }

            ViewBag.SDT = sdt;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> LichHen()
        {
            await DemThongBao();

            var id = HttpContext.Session.GetInt32("IdBenhNhan");
            if (id == null)
                return RedirectToAction("Login", "Account");

            var data = await (from lh in _context.LichHens.AsNoTracking()
                              join bs in _context.BacSis.AsNoTracking() on lh.IdBacSi equals bs.IdBacSi
                              where lh.IdBenhNhan == id.Value
                              orderby lh.Ngay descending, lh.Gio descending
                              select new PhieuKhamViewModel
                              {
                                  IdLichHen = lh.IdLichHen,
                                  PhieuKham = lh.PhieuKham ?? "",
                                  TenBacSi = bs.HoTen,
                                  NgayKham = lh.Ngay,
                                  GioKham = lh.Gio,
                                  TrangThai = lh.TrangThai
                              }).ToListAsync();

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> HuyLich(int idLichHen, string lydo)
        {
            if (idLichHen <= 0 || string.IsNullOrWhiteSpace(lydo))
                return RedirectToAction("LichHen");

            var trangThai = await _context.LichHens
                .AsNoTracking()
                .Where(x => x.IdLichHen == idLichHen)
                .Select(x => x.TrangThai)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(trangThai) || trangThai != "Đã đặt")
                return RedirectToAction("TraCuu");

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_HuyLich @p0, @p1, @p2",
                idLichHen, lydo, "BenhNhan");

            return RedirectToAction("LichHen");
        }

        [HttpGet]
        public async Task<IActionResult> ThongBao()
        {
            await DemThongBao();

            var id = HttpContext.Session.GetInt32("IdBenhNhan");
            if (id == null)
                return RedirectToAction("Login", "Account");

            var list = await _context.ThongBaos
                .AsNoTracking()
                .Where(x => x.IdBenhNhan == id.Value)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> DocThongBao(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb != null)
            {
                tb.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ThongBao");
        }
    }
}
