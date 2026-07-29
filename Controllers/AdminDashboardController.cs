using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;
using BanXePKL.Models;

namespace BanXePKL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly BanXePKLContext _context;

        public AdminDashboardController(BanXePKLContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tongDoanhThu = await _context.DonHang
                .Where(d => d.TrangThaiDonHang != "DaHuy")
                .SumAsync(d => (decimal?)d.TongTien) ?? 0;

            var tongDonHang = await _context.DonHang.CountAsync();
            var tongKhachHang = await _context.NguoiDung.CountAsync(u => u.VaiTroId == 2);
            var tongXe = await _context.Xe.CountAsync();

            // Doanh thu 6 tháng gần nhất
            var thang6ThangGanNhat = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var doanhThuTheoThang = new List<ThongKeThangViewModel>();
            foreach (var thang in thang6ThangGanNhat)
            {
                var doanhThu = await _context.DonHang
                    .Where(d => d.TrangThaiDonHang != "DaHuy"
                                && d.NgayDat.Month == thang.Month
                                && d.NgayDat.Year == thang.Year)
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;

                doanhThuTheoThang.Add(new ThongKeThangViewModel
                {
                    Thang = $"{thang.Month}/{thang.Year}",
                    DoanhThu = doanhThu
                });
            }

            // Top 5 xe bán chạy nhất (dựa trên số lượng đã bán trong ChiTietDonHang)
            var xeBanChay = await _context.ChiTietDonHang
                .GroupBy(ct => ct.XeId)
                .Select(g => new { XeId = g.Key, SoLuongBan = g.Sum(x => x.SoLuong) })
                .OrderByDescending(g => g.SoLuongBan)
                .Take(5)
                .Join(_context.Xe, g => g.XeId, xe => xe.XeId, (g, xe) => new XeBanChayViewModel
                {
                    TenXe = xe.TenXe,
                    SoLuongBan = g.SoLuongBan
                })
                .ToListAsync();

            var viewModel = new ThongKeViewModel
            {
                TongDoanhThu = tongDoanhThu,
                TongDonHang = tongDonHang,
                TongKhachHang = tongKhachHang,
                TongXe = tongXe,
                DoanhThuTheoThang = doanhThuTheoThang,
                XeBanChay = xeBanChay
            };

            return View(viewModel);
        }
    }
}
