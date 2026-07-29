
using System;

namespace BanXePKL_Project.Models
{
    public partial class DanhGia
    {
        public int DanhGiaId { get; set; }
        public int XeId { get; set; }
        public int UserId { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; }

        public virtual Xe Xe { get; set; } = null!;
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
