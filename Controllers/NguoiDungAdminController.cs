using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;

namespace BanXePKL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NguoiDungAdminController : Controller
    {
        private readonly BanXePKLContext _context;

        public NguoiDungAdminController(BanXePKLContext context)
        {
            _context = context;
        }

        // GET: /NguoiDungAdmin
        public async Task<IActionResult> Index(string? keyword)
        {
            // Chỉ hiển thị khách hàng (VaiTroId = 2), không hiển thị tài khoản Admin khác
            var query = _context.NguoiDung.Where(u => u.VaiTroId == 2);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(u => u.HoTen.Contains(keyword) || u.Email.Contains(keyword));

            var danhSach = await query.OrderByDescending(u => u.NgayTao).ToListAsync();
            ViewBag.Keyword = keyword;
            return View(danhSach);
        }

        // POST: /NguoiDungAdmin/KhoaMoKhoa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaMoKhoa(int userId)
        {
            var nguoiDung = await _context.NguoiDung.FindAsync(userId);
            if (nguoiDung != null)
            {
                nguoiDung.TrangThai = !nguoiDung.TrangThai;
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = nguoiDung.TrangThai
                    ? $"Đã mở khoá tài khoản của {nguoiDung.HoTen}."
                    : $"Đã khoá tài khoản của {nguoiDung.HoTen}.";
            }
            return RedirectToAction("Index");
        }
    }
}
