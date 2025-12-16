using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class BacSiController : Controller
    {
        private readonly ClinicDbContext _context;

        public BacSiController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? ck)
        {
            var vm = new ChuyenKhoaViewModel();

            vm.ChuyenKhoas = await _context.BacSis
                .AsNoTracking()
                .Select(x => x.ChuyenKhoa)
                .Where(x => x != null && x != "")
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (!string.IsNullOrEmpty(ck))
            {
                vm.SelectedChuyenKhoa = ck;
                vm.BacSis = await _context.BacSis
                    .AsNoTracking()
                    .Where(x => x.ChuyenKhoa == ck)
                    .Select(x => new BacSi
                    {
                        IdBacSi = x.IdBacSi,
                        HoTen = x.HoTen,
                        ChuyenKhoa = x.ChuyenKhoa,
                        KinhNghiem = x.KinhNghiem,
                        MoTa = x.MoTa ?? "",
                        HinhAnh = x.HinhAnh ?? ""
                    })
                    .ToListAsync();
            }
            else
            {
                vm.BacSis = await _context.BacSis
                    .AsNoTracking()
                    .Select(x => new BacSi
                    {
                        IdBacSi = x.IdBacSi,
                        HoTen = x.HoTen,
                        ChuyenKhoa = x.ChuyenKhoa,
                        KinhNghiem = x.KinhNghiem,
                        MoTa = x.MoTa ?? "",
                        HinhAnh = x.HinhAnh ?? ""
                    })
                    .ToListAsync();
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var bs = await _context.BacSis
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdBacSi == id);

            if (bs == null)
                return NotFound();

            if (bs.MoTa == null) bs.MoTa = "";
            if (bs.HinhAnh == null) bs.HinhAnh = "";

            return View(bs);
        }
    }
}
