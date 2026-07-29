using BanXePKL_Project.Model;
using System;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class GioHang
    {
        public GioHang()
        {
            GioHangChiTiet = new HashSet<GioHangChiTiet>();
        }

        public int GioHangId { get; set; }
        public int UserId { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual ICollection<GioHangChiTiet> GioHangChiTiet { get; set; }
    }
}
