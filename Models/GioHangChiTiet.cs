using BanXePKL_Project.Model;

namespace BanXePKL_Project.Models
{
    public partial class GioHangChiTiet
    {
        public int Id { get; set; }
        public int GioHangId { get; set; }
        public int XeId { get; set; }
        public int SoLuong { get; set; }

        public virtual GioHang GioHang { get; set; } = null!;
        public virtual Xe Xe { get; set; } = null!;
    }
}
