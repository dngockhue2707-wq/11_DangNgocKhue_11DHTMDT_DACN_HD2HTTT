using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly ClinicDbContext _context;

        public ThongBaoController(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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
    }
}
