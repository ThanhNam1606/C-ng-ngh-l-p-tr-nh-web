using System;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class Xe
    {
        public Xe()
        {
            AnhXe = new HashSet<AnhXe>();
            ChiTietDonHang = new HashSet<ChiTietDonHang>();
            DanhGia = new HashSet<DanhGia>();
            GioHangChiTiet = new HashSet<GioHangChiTiet>();
        }

        public int XeId { get; set; }
        public string TenXe { get; set; } = null!;
        public int HangXeId { get; set; }
        public int DanhMucId { get; set; }
        public int PhanKhoi { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public int? NamSanXuat { get; set; }
        public string? MauSac { get; set; }
        public string? ThongSoKyThuat { get; set; }
        public string? MoTa { get; set; }
        public string? AnhDaiDien { get; set; }
        public DateTime NgayThem { get; set; }
        public bool TrangThai { get; set; }

        public virtual HangXe HangXe { get; set; } = null!;
        public virtual DanhMuc DanhMuc { get; set; } = null!;
        public virtual ICollection<AnhXe> AnhXe { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; }
        public virtual ICollection<DanhGia> DanhGia { get; set; }
        public virtual ICollection<GioHangChiTiet> GioHangChiTiet { get; set; }
    }
}
