using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;

namespace WebPhongKham.Controllers
{
    public class SMSController : Controller
    {
        private readonly ClinicDbContext _context;

        public SMSController(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.SMS_Logs
                .AsNoTracking()
                .OrderByDescending(x => x.NgayGui)
                .ToListAsync();

            return View(list);
        }
    }
}
