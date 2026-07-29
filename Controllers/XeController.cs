using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;
using BanXePKL.Models;

namespace BanXePKL.Controllers
{
    public class XeController : Controller
    {
        private readonly BanXePKLContext _context;
        private const int PageSize = 8; // số xe hiển thị mỗi trang

        public XeController(BanXePKLContext context)
        {
            _context = context;
        }

        // GET: /Xe
        // Trang danh sách xe: hỗ trợ lọc theo hãng/danh mục/khoảng giá,
        // tìm kiếm theo tên, sắp xếp và phân trang.
        public async Task<IActionResult> Index(
            int? hangXeId,
            int? danhMucId,
            decimal? giaMin,
            decimal? giaMax,
            string? keyword,
            string sortOrder = "moi_nhat",
            int page = 1)
        {
            var query = _context.Xe
                .Include(x => x.HangXe)
                .Include(x => x.DanhMuc)
                .Where(x => x.TrangThai == true)
                .AsQueryable();

            if (hangXeId.HasValue)
                query = query.Where(x => x.HangXeId == hangXeId.Value);

            if (danhMucId.HasValue)
                query = query.Where(x => x.DanhMucId == danhMucId.Value);

            if (giaMin.HasValue)
                query = query.Where(x => x.GiaBan >= giaMin.Value);

            if (giaMax.HasValue)
                query = query.Where(x => x.GiaBan <= giaMax.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.TenXe.Contains(keyword));

            query = sortOrder switch
            {
                "gia_tang" => query.OrderBy(x => x.GiaBan),
                "gia_giam" => query.OrderByDescending(x => x.GiaBan),
                "ten_az" => query.OrderBy(x => x.TenXe),
                _ => query.OrderByDescending(x => x.NgayThem) // mặc định: mới nhất
            };

            int totalItems = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Max(1, Math.Min(page, totalPages));

            var danhSachXe = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new XeFilterViewModel
            {
                DanhSachXe = danhSachXe,
                DanhSachHangXe = await _context.HangXe.OrderBy(h => h.TenHang).ToListAsync(),
                DanhSachDanhMuc = await _context.DanhMuc.OrderBy(d => d.TenDanhMuc).ToListAsync(),
                HangXeIdDaChon = hangXeId,
                DanhMucIdDaChon = danhMucId,
                GiaMin = giaMin,
                GiaMax = giaMax,
                Keyword = keyword,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: /Xe/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var xe = await _context.Xe
                .Include(x => x.HangXe)
                .Include(x => x.DanhMuc)
                .Include(x => x.AnhXe)
                .Include(x => x.DanhGia)
                .FirstOrDefaultAsync(x => x.XeId == id);

            if (xe == null)
                return NotFound();

            // Gợi ý xe liên quan: cùng danh mục, khác chính nó
            var xeLienQuan = await _context.Xe
                .Where(x => x.DanhMucId == xe.DanhMucId
                            && x.XeId != xe.XeId
                            && x.TrangThai == true)
                .Take(4)
                .ToListAsync();

            ViewBag.XeLienQuan = xeLienQuan;

            return View(xe);
        }
    }
}
