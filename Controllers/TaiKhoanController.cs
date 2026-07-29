using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;
using BanXePKL.Models;

namespace BanXePKL.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly BanXePKLContext _context;
        // PasswordHasher: dùng chuẩn mã hoá mật khẩu của Microsoft, không cần cài thêm Identity đầy đủ
        private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

        public TaiKhoanController(BanXePKLContext context)
        {
            _context = context;
        }

        // ===================== ĐĂNG KÝ =====================

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.NguoiDung.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký, vui lòng dùng email khác.");
                return View(model);
            }

            var nguoiDung = new NguoiDung
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                VaiTroId = 2, // mặc định: KhachHang (VaiTroId=1 là Admin, xem seed data ở phần Người 1)
                TrangThai = true
            };
            nguoiDung.MatKhauHash = _passwordHasher.HashPassword(nguoiDung, model.MatKhau);

            _context.NguoiDung.Add(nguoiDung);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }

        // ===================== ĐĂNG NHẬP =====================

        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nguoiDung = await _context.NguoiDung
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (nguoiDung == null || !nguoiDung.TrangThai)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khoá.");
                return View(model);
            }

            var ketQua = _passwordHasher.VerifyHashedPassword(nguoiDung, nguoiDung.MatKhauHash, model.MatKhau);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Claims: thông tin đính kèm trong cookie đăng nhập, dùng lại ở mọi Controller qua User.Xxx
            var claims = new List<Claim>
            {
                new Claim("UserId", nguoiDung.UserId.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro?.TenVaiTro ?? "KhachHang")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = model.GhiNhoDangNhap });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (nguoiDung.VaiTro?.TenVaiTro == "Admin")
                return RedirectToAction("Index", "AdminDashboard"); // Người 5 sẽ tạo Controller này

            return RedirectToAction("Index", "Home");
        }

        // ===================== ĐĂNG XUẤT =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ===================== THÔNG TIN CÁ NHÂN =====================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ThongTin()
        {
            var userId = LayUserId();
            var nguoiDung = await _context.NguoiDung.FindAsync(userId);
            if (nguoiDung == null) return NotFound();
            return View(nguoiDung);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTin(NguoiDung model)
        {
            var userId = LayUserId();
            var nguoiDung = await _context.NguoiDung.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            nguoiDung.HoTen = model.HoTen;
            nguoiDung.SoDienThoai = model.SoDienThoai;
            nguoiDung.DiaChi = model.DiaChi;

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("ThongTin");
        }

        // ===================== ĐỔI MẬT KHẨU =====================

        [Authorize]
        [HttpGet]
        public IActionResult DoiMatKhau() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = LayUserId();
            var nguoiDung = await _context.NguoiDung.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            var ketQua = _passwordHasher.VerifyHashedPassword(nguoiDung, nguoiDung.MatKhauHash, model.MatKhauCu);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            nguoiDung.MatKhauHash = _passwordHasher.HashPassword(nguoiDung, model.MatKhauMoi);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("ThongTin");
        }

        // ===================== TRUY CẬP BỊ TỪ CHỐI =====================

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // Lấy UserId hiện tại từ Claims (dùng cho mọi Controller khác qua User.FindFirst("UserId"))
        private int LayUserId()
        {
            return int.Parse(User.FindFirst("UserId")!.Value);
        }
    }
}
