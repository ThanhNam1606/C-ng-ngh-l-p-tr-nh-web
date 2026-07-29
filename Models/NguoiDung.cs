using BanXePKL_Project.Model;
using System;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class NguoiDung
    {
        public NguoiDung()
        {
            DanhGia = new HashSet<DanhGia>();
            DonHang = new HashSet<DonHang>();
            GioHang = new HashSet<GioHang>();
        }

        public int UserId { get; set; }
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MatKhauHash { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public int VaiTroId { get; set; }
        public DateTime NgayTao { get; set; }
        public bool TrangThai { get; set; }

        public virtual VaiTro VaiTro { get; set; } = null!;
        public virtual ICollection<DanhGia> DanhGia { get; set; }
        public virtual ICollection<DonHang> DonHang { get; set; }
        public virtual ICollection<GioHang> GioHang { get; set; }
    }
}
