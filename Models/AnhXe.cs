using BanXePKL_Project.Models;

namespace BanXePKL_Project.Models
{
    public partial class AnhXe
    {
        public int AnhId { get; set; }
        public int XeId { get; set; }
        public string DuongDanAnh { get; set; } = null!;
        public bool LaAnhChinh { get; set; }

        public virtual Xe Xe { get; set; } = null!;
    }
}
