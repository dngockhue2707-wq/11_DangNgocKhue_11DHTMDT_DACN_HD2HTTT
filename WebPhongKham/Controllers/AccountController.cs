using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhongKham.Data;
using WebPhongKham.Models;
using WebPhongKham.Models.ViewModels;

namespace WebPhongKham.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClinicDbContext _context;

        public AccountController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var tk = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Email == model.Email &&
                    x.MatKhau == model.MatKhau
                );

            if (tk == null)
            {
                model.Message = "Sai email hoặc mật khẩu!";
                return View(model);
            }

            if (tk.TrangThai == 0)
            {
                model.Message = "Tài khoản đã bị khóa!";
                return View(model);
            }

            var vaiTro = tk.VaiTro ?? "";

            HttpContext.Session.SetString("Email", tk.Email ?? "");
            HttpContext.Session.SetString("VaiTro", vaiTro);

            if (tk.IdBenhNhan.HasValue)
                HttpContext.Session.SetInt32("IdBenhNhan", tk.IdBenhNhan.Value);

            if (tk.IdBacSi.HasValue)
                HttpContext.Session.SetInt32("IdBacSi", tk.IdBacSi.Value);

            return vaiTro switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "BacSi" => RedirectToAction("LichKham", "BacSiDashboard"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var hoTen = model.HoTen ?? "";
                var ngaySinh = (object?)model.NgaySinh ?? DBNull.Value;
                var gioiTinh = (object?)model.GioiTinh ?? DBNull.Value;
                var diaChi = (object?)model.DiaChi ?? DBNull.Value;
                var sdt = model.SDT ?? "";
                var email = model.Email ?? "";
                var matKhau = model.MatKhau ?? "";

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_DangKyBenhNhan @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                    hoTen, ngaySinh, gioiTinh, diaChi, sdt, email, matKhau
                );

                TempData["Success"] = "Đăng ký thành công! Mời bạn đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> HoSo()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var vaiTro = HttpContext.Session.GetString("VaiTro") ?? "";

            if (vaiTro == "User")
            {
                var data = await (
                    from tk in _context.TaiKhoans.AsNoTracking()
                    join bn in _context.BenhNhans.AsNoTracking()
                        on tk.IdBenhNhan equals bn.IdBenhNhan
                    where tk.Email == email
                    select new
                    {
                        Email = tk.Email,
                        HoTen = bn.HoTen,
                        SDT = bn.SDT,
                        DiaChi = bn.DiaChi,
                        GioiTinh = bn.GioiTinh,
                        NgaySinh = bn.NgaySinh
                    }
                ).FirstOrDefaultAsync();

                return View(data);
            }

            if (vaiTro == "BacSi")
            {
                var data = await (
                    from tk in _context.TaiKhoans.AsNoTracking()
                    join bs in _context.BacSis.AsNoTracking()
                        on tk.IdBacSi equals bs.IdBacSi
                    where tk.Email == email
                    select new
                    {
                        Email = tk.Email,
                        TenBacSi = bs.HoTen,
                        ChuyenKhoa = bs.ChuyenKhoa,
                        KinhNghiem = bs.KinhNghiem,
                        MoTa = bs.MoTa
                    }
                ).FirstOrDefaultAsync();

                return View(data);
            }

            return View(null);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
