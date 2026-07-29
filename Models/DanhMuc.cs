using BanXePKL_Project.Model;
using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class DanhMuc
    {
        public DanhMuc()
        {
            Xe = new HashSet<Xe>();
        }

        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = null!;

        public virtual ICollection<Xe> Xe { get; set; }
    }
}
