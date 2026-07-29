
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;

namespace BanXePKL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DonHangAdminController : Controller
    {
        private readonly BanXePKLContext _context;

        public DonHangAdminController(BanXePKLContext context)
        {
            _context = context;
        }

        // GET: /DonHangAdmin?trangThai=ChoXacNhan
        public async Task<IActionResult> Index(string? trangThai)
        {
            // Lưu ý: tên navigation property "NguoiDung" phụ thuộc vào cách EF Core
            // đặt tên khi Scaffold. Nếu Scaffold ra tên khác (VD "NguoiDungNavigation"),
            // đổi lại cho khớp.
            var query = _context.DonHang.Include(d => d.NguoiDung).AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(d => d.TrangThaiDonHang == trangThai);

            var danhSach = await query.OrderByDescending(d => d.NgayDat).ToListAsync();
            ViewBag.TrangThai = trangThai;
            return View(danhSach);
        }

        // GET: /DonHangAdmin/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            var donHang = await _context.DonHang
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(d => d.DonHangId == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        // POST: /DonHangAdmin/CapNhatTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int donHangId, string trangThaiMoi)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(d => d.DonHangId == donHangId);

            if (donHang == null) return NotFound();

            // Nếu admin huỷ đơn (mà đơn đó chưa từng bị huỷ trước đó) thì hoàn lại tồn kho
            if (trangThaiMoi == "DaHuy" && donHang.TrangThaiDonHang != "DaHuy")
            {
                foreach (var ct in donHang.ChiTietDonHang)
                {
                    ct.Xe.SoLuongTon += ct.SoLuong;
                }
            }

            donHang.TrangThaiDonHang = trangThaiMoi;
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã cập nhật trạng thái đơn hàng #{donHangId}.";
            return RedirectToAction("ChiTiet", new { id = donHangId });
        }
    }
}
