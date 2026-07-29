using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;

namespace BanXePKL.Controllers
{
    public class GioHangController : Controller
    {
        private readonly BanXePKLContext _context;

        public GioHangController(BanXePKLContext context)
        {
            _context = context;
        }

        // TODO (Người 4): Thay hàm này bằng cách lấy UserId thật từ Claims
        // sau khi tích hợp ASP.NET Core Identity, ví dụ:
        // int.Parse(User.FindFirst("UserId")?.Value ?? "0")
        // Hiện tại đang tạm lấy từ Session để 2 module chạy độc lập trước.
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Lấy giỏ hàng hiện có của user, hoặc tạo mới nếu chưa có
        private async Task<GioHang> LayHoacTaoGioHangAsync(int userId)
        {
            var gioHang = await _context.GioHang
                .Include(g => g.GioHangChiTiet)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang { UserId = userId };
                _context.GioHang.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            return gioHang;
        }

        // GET: /GioHang
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/GioHang" });

            var gioHang = await LayHoacTaoGioHangAsync(userId.Value);
            return View(gioHang);
        }

        // POST: /GioHang/ThemVaoGio
        // Được gọi từ nút "Thêm vào giỏ hàng" ở trang Details của Người 2
        [HttpPost]
        public async Task<IActionResult> ThemVaoGio(int xeId, int soLuong = 1)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan",
                    new { returnUrl = Url.Action("Details", "Xe", new { id = xeId }) });

            var xe = await _context.Xe.FindAsync(xeId);
            if (xe == null || soLuong <= 0 || xe.SoLuongTon < soLuong)
            {
                TempData["Loi"] = "Xe không tồn tại hoặc không đủ số lượng tồn kho.";
                return RedirectToAction("Details", "Xe", new { id = xeId });
            }

            var gioHang = await LayHoacTaoGioHangAsync(userId.Value);

            var chiTiet = gioHang.GioHangChiTiet.FirstOrDefault(ct => ct.XeId == xeId);
            if (chiTiet != null)
            {
                chiTiet.SoLuong += soLuong;
            }
            else
            {
                _context.GioHangChiTiet.Add(new GioHangChiTiet
                {
                    GioHangId = gioHang.GioHangId,
                    XeId = xeId,
                    SoLuong = soLuong
                });
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm \"{xe.TenXe}\" vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        // POST: /GioHang/CapNhatSoLuong
        [HttpPost]
        public async Task<IActionResult> CapNhatSoLuong(int chiTietId, int soLuong)
        {
            var chiTiet = await _context.GioHangChiTiet.FindAsync(chiTietId);
            if (chiTiet != null && soLuong > 0)
            {
                chiTiet.SoLuong = soLuong;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /GioHang/XoaSanPham
        [HttpPost]
        public async Task<IActionResult> XoaSanPham(int chiTietId)
        {
            var chiTiet = await _context.GioHangChiTiet.FindAsync(chiTietId);
            if (chiTiet != null)
            {
                _context.GioHangChiTiet.Remove(chiTiet);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
