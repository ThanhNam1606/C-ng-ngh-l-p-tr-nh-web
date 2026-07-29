using System.Collections.Generic;

namespace BanXePKL_Project.Models
{
    public partial class VaiTro
    {
        public VaiTro()
        {
            NguoiDung = new HashSet<NguoiDung>();
        }

        public int VaiTroId { get; set; }
        public string TenVaiTro { get; set; } = null!;

        public virtual ICollection<NguoiDung> NguoiDung { get; set; }
    }
}
