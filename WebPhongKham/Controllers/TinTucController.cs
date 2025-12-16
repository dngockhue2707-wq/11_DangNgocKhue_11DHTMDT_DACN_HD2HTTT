using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;

namespace WebPhongKham.Controllers
{
    public class TinTucController : Controller
    {
        private readonly ClinicDbContext _context;

        public TinTucController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? loai)
        {
            var query = _context.TinTucs
                .AsNoTracking()
                .Where(x => x.TrangThai == true);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x =>
                    (x.TieuDe != null && x.TieuDe.Contains(q)) ||
                    (x.MoTaNgan != null && x.MoTaNgan.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(loai))
            {
                query = query.Where(x => x.LoaiTin == loai);
            }

            var list = await query
                .OrderByDescending(x => x.NgayDang)
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Loai = loai;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var tin = await _context.TinTucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.TrangThai);

            if (tin == null)
                return NotFound();

            return View(tin);
        }
    }
}
