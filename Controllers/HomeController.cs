using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanXePKL.Data;

namespace BanXePKL.Controllers
{
    public class HomeController : Controller
    {
        private readonly BanXePKLContext _context;

        public HomeController(BanXePKLContext context)
        {
            _context = context;
        }

        // GET: /  hoặc /Home/Index
        public async Task<IActionResult> Index()
        {
            // Xe mới nhất để làm nổi bật trên trang chủ
            var xeNoiBat = await _context.Xe
                .Include(x => x.HangXe)
                .Where(x => x.TrangThai == true)
                .OrderByDescending(x => x.NgayThem)
                .Take(6)
                .ToListAsync();

            var danhMuc = await _context.DanhMuc.ToListAsync();

            ViewBag.DanhMuc = danhMuc;

            return View(xeNoiBat);
        }
    }
}
