using BanXePKL_Project.Model;
using System;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class DonHang
    {
        public DonHang()
        {
            ChiTietDonHang = new HashSet<ChiTietDonHang>();
        }

        public int DonHangId { get; set; }
        public int UserId { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string DiaChiGiao { get; set; } = null!;
        public string SoDienThoaiNhan { get; set; } = null!;
        public string PhuongThucThanhToan { get; set; } = null!;
        public string TrangThaiDonHang { get; set; } = null!;
        public string? GhiChu { get; set; }

        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}
