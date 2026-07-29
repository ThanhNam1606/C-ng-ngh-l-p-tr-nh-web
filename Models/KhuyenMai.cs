using System;

namespace BanXePKL_Project.Models
{
    public partial class KhuyenMai
    {
        public int KhuyenMaiId { get; set; }
        public string MaCode { get; set; } = null!;
        public int PhanTramGiam { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public bool TrangThai { get; set; }
    }
}
