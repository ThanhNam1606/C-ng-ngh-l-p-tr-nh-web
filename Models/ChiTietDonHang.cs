using BanXePKL_Project.Model;

namespace BanXePKL_Project.Models
{
    public partial class ChiTietDonHang
    {
        public int Id { get; set; }
        public int DonHangId { get; set; }
        public int XeId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; } // cột tính toán (computed column) trong SQL

        public virtual DonHang DonHang { get; set; } = null!;
        public virtual Xe Xe { get; set; } = null!;
    }
}
