using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;
using BanXePKL.Models;

namespace BanXePKL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class XeAdminController : Controller
    {
        private readonly BanXePKLContext _context;
        private readonly IWebHostEnvironment _env;

        public XeAdminController(BanXePKLContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /XeAdmin
        public async Task<IActionResult> Index(string? keyword)
        {
            var query = _context.Xe.Include(x => x.HangXe).Include(x => x.DanhMuc).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.TenXe.Contains(keyword));

            var danhSach = await query.OrderByDescending(x => x.NgayThem).ToListAsync();
            ViewBag.Keyword = keyword;
            return View(danhSach);
        }

        // GET: /XeAdmin/Create
        public async Task<IActionResult> Create()
        {
            await NapDropdown();
            return View(new XeAdminViewModel());
        }

        // POST: /XeAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(XeAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await NapDropdown();
                return View(model);
            }

            var xe = new Xe
            {
                TenXe = model.TenXe,
                HangXeId = model.HangXeId,
                DanhMucId = model.DanhMucId,
                PhanKhoi = model.PhanKhoi,
                GiaBan = model.GiaBan,
                SoLuongTon = model.SoLuongTon,
                NamSanXuat = model.NamSanXuat,
                MauSac = model.MauSac,
                ThongSoKyThuat = model.ThongSoKyThuat,
                MoTa = model.MoTa,
                NgayThem = DateTime.Now,
                TrangThai = model.TrangThai
            };

            if (model.AnhDaiDienFile != null)
                xe.AnhDaiDien = await LuuAnhAsync(model.AnhDaiDienFile);

            _context.Xe.Add(xe);
            await _context.SaveChangesAsync();

            if (model.AnhPhuFiles != null && model.AnhPhuFiles.Any())
            {
                foreach (var file in model.AnhPhuFiles)
                {
                    var duongDan = await LuuAnhAsync(file);
                    _context.AnhXe.Add(new AnhXe { XeId = xe.XeId, DuongDanAnh = duongDan });
                }
                await _context.SaveChangesAsync();
            }

            TempData["ThongBao"] = $"Đã thêm xe \"{xe.TenXe}\" thành công.";
            return RedirectToAction("Index");
        }

        // GET: /XeAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe == null) return NotFound();

            await NapDropdown();

            var model = new XeAdminViewModel
            {
                XeId = xe.XeId,
                TenXe = xe.TenXe,
                HangXeId = xe.HangXeId,
                DanhMucId = xe.DanhMucId,
                PhanKhoi = xe.PhanKhoi,
                GiaBan = xe.GiaBan,
                SoLuongTon = xe.SoLuongTon,
                NamSanXuat = xe.NamSanXuat,
                MauSac = xe.MauSac,
                ThongSoKyThuat = xe.ThongSoKyThuat,
                MoTa = xe.MoTa,
                AnhDaiDienHienTai = xe.AnhDaiDien,
                TrangThai = xe.TrangThai
            };

            return View(model);
        }

        // POST: /XeAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, XeAdminViewModel model)
        {
            if (id != model.XeId) return NotFound();

            if (!ModelState.IsValid)
            {
                await NapDropdown();
                return View(model);
            }

            var xe = await _context.Xe.FindAsync(id);
            if (xe == null) return NotFound();

            xe.TenXe = model.TenXe;
            xe.HangXeId = model.HangXeId;
            xe.DanhMucId = model.DanhMucId;
            xe.PhanKhoi = model.PhanKhoi;
            xe.GiaBan = model.GiaBan;
            xe.SoLuongTon = model.SoLuongTon;
            xe.NamSanXuat = model.NamSanXuat;
            xe.MauSac = model.MauSac;
            xe.ThongSoKyThuat = model.ThongSoKyThuat;
            xe.MoTa = model.MoTa;
            xe.TrangThai = model.TrangThai;

            if (model.AnhDaiDienFile != null)
                xe.AnhDaiDien = await LuuAnhAsync(model.AnhDaiDienFile);

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Cập nhật xe thành công.";
            return RedirectToAction("Index");
        }

        // POST: /XeAdmin/Delete/5
        // Lưu ý: KHÔNG xoá cứng khỏi database vì xe có thể đã nằm trong đơn hàng cũ
        // (xoá cứng sẽ vi phạm khoá ngoại / làm mất lịch sử đơn hàng).
        // Thay vào đó, chuyển TrangThai = false để ẩn khỏi cửa hàng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe != null)
            {
                xe.TrangThai = false;
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã ẩn xe \"{xe.TenXe}\" khỏi cửa hàng.";
            }
            return RedirectToAction("Index");
        }

        private async Task NapDropdown()
        {
            ViewBag.DanhSachHangXe = await _context.HangXe.OrderBy(h => h.TenHang).ToListAsync();
            ViewBag.DanhSachDanhMuc = await _context.DanhMuc.OrderBy(d => d.TenDanhMuc).ToListAsync();
        }

        private async Task<string> LuuAnhAsync(IFormFile file)
        {
            var thuMuc = Path.Combine(_env.WebRootPath, "images", "xe");
            Directory.CreateDirectory(thuMuc);

            var tenFile = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var duongDanDayDu = Path.Combine(thuMuc, tenFile);

            using var stream = new FileStream(duongDanDayDu, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/xe/{tenFile}";
        }
    }
}
