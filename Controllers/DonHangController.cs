using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;
using BanXePKL.Models;

namespace BanXePKL.Controllers
{
    public class DonHangController : Controller
    {
        private readonly BanXePKLContext _context;

        public DonHangController(BanXePKLContext context)
        {
            _context = context;
        }

        // TODO (Người 4): thay bằng UserId lấy từ Claims sau khi có Identity thật
        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserId");

        // GET: /DonHang/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/DonHang/Checkout" });

            var gioHang = await _context.GioHang
                .Include(g => g.GioHangChiTiet)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null || !gioHang.GioHangChiTiet.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống, vui lòng chọn xe trước khi thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }

            var nguoiDung = await _context.NguoiDung.FindAsync(userId.Value);

            var viewModel = new DatHangViewModel
            {
                GioHang = gioHang,
                DiaChiGiao = nguoiDung?.DiaChi,
                SoDienThoaiNhan = nguoiDung?.SoDienThoai,
                TongTien = gioHang.GioHangChiTiet.Sum(ct => ct.SoLuong * ct.Xe.GiaBan)
            };

            return View(viewModel);
        }

        // POST: /DonHang/XacNhanDatHang
        [HttpPost]
        public async Task<IActionResult> XacNhanDatHang(DatHangViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (string.IsNullOrWhiteSpace(model.DiaChiGiao) || string.IsNullOrWhiteSpace(model.SoDienThoaiNhan))
            {
                TempData["Loi"] = "Vui lòng nhập đầy đủ địa chỉ và số điện thoại nhận hàng.";
                return RedirectToAction("Checkout");
            }

            var gioHang = await _context.GioHang
                .Include(g => g.GioHangChiTiet)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null || !gioHang.GioHangChiTiet.Any())
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "GioHang");
            }

            // Kiểm tra lại tồn kho trước khi đặt, tránh bán vượt số lượng thực tế
            foreach (var ct in gioHang.GioHangChiTiet)
            {
                if (ct.Xe.SoLuongTon < ct.SoLuong)
                {
                    TempData["Loi"] = $"Xe \"{ct.Xe.TenXe}\" chỉ còn {ct.Xe.SoLuongTon} chiếc, vui lòng cập nhật lại giỏ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = new DonHang
                {
                    UserId = userId.Value,
                    NgayDat = DateTime.Now,
                    DiaChiGiao = model.DiaChiGiao!,
                    SoDienThoaiNhan = model.SoDienThoaiNhan!,
                    PhuongThucThanhToan = string.IsNullOrWhiteSpace(model.PhuongThucThanhToan) ? "COD" : model.PhuongThucThanhToan,
                    TrangThaiDonHang = "ChoXacNhan",
                    GhiChu = model.GhiChu,
                    TongTien = gioHang.GioHangChiTiet.Sum(ct => ct.SoLuong * ct.Xe.GiaBan)
                };
                _context.DonHang.Add(donHang);
                await _context.SaveChangesAsync();

                foreach (var ct in gioHang.GioHangChiTiet)
                {
                    _context.ChiTietDonHang.Add(new ChiTietDonHang
                    {
                        DonHangId = donHang.DonHangId,
                        XeId = ct.XeId,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.Xe.GiaBan
                    });

                    // Trừ số lượng tồn kho ngay khi đặt hàng
                    ct.Xe.SoLuongTon -= ct.SoLuong;
                }

                // Xoá sạch giỏ hàng sau khi đặt thành công
                _context.GioHangChiTiet.RemoveRange(gioHang.GioHangChiTiet);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // TODO: Nếu PhuongThucThanhToan là "VNPay" hoặc "Momo",
                // redirect người dùng sang URL cổng thanh toán tương ứng tại đây,
                // ví dụ: return Redirect(vnPayService.TaoUrlThanhToan(donHang));

                return RedirectToAction("DatHangThanhCong", new { id = donHang.DonHangId });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Loi"] = "Có lỗi xảy ra trong quá trình đặt hàng, vui lòng thử lại.";
                return RedirectToAction("Checkout");
            }
        }

        // GET: /DonHang/DatHangThanhCong/5
        public async Task<IActionResult> DatHangThanhCong(int id)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(d => d.DonHangId == id);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // GET: /DonHang/LichSu
        public async Task<IActionResult> LichSu()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/DonHang/LichSu" });

            var danhSach = await _context.DonHang
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: /DonHang/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            var userId = GetCurrentUserId();

            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(d => d.DonHangId == id);

            // Chỉ cho phép xem đơn hàng của chính mình
            if (donHang == null || donHang.UserId != userId)
                return NotFound();

            return View(donHang);
        }
    }
}
