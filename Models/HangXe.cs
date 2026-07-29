using BanXePKL_Project.Model;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class HangXe
    {
        public HangXe()
        {
            Xe = new HashSet<Xe>();
        }

        public int HangXeId { get; set; }
        public string TenHang { get; set; } = null!;
        public string? QuocGia { get; set; }
        public string? LogoUrl { get; set; }
        public string? MoTa { get; set; }

        public virtual ICollection<Xe> Xe { get; set; }
    }
}
