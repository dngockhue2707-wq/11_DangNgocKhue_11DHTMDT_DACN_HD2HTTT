using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;

namespace WebPhongKham.Controllers
{
    public class BacSiDashboardController : Controller
    {
        private readonly ClinicDbContext _context;

        public BacSiDashboardController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> LichKham(DateTime? ngay)
        {
            var idBacSi = HttpContext.Session.GetInt32("IdBacSi");
            if (idBacSi == null)
                return RedirectToAction("Login", "Account");

            var date = ngay ?? DateTime.Today;

            var list = await _context.LichHens
                .AsNoTracking()
                .Where(x => x.IdBacSi == idBacSi.Value && x.Ngay.Date == date.Date)
                .OrderBy(x => x.Gio)
                .ToListAsync();

            ViewBag.Ngay = date;
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> DaKham(int idLichHen)
        {
            var idBacSi = HttpContext.Session.GetInt32("IdBacSi");
            if (idBacSi == null)
                return RedirectToAction("Login", "Account");

            await _context.Database.ExecuteSqlRawAsync("EXEC SP_DaKham @p0", idLichHen);

            return RedirectToAction("LichKham");
        }

        [HttpPost]
        public async Task<IActionResult> HuyLich(int idLichHen, string lydo)
        {
            var idBacSi = HttpContext.Session.GetInt32("IdBacSi");
            if (idBacSi == null)
                return RedirectToAction("Login", "Account");

            await _context.Database.ExecuteSqlRawAsync("EXEC SP_HuyLich @p0, @p1, @p2", idLichHen, lydo, "BacSi");

            return RedirectToAction("LichKham");
        }
    }
}
