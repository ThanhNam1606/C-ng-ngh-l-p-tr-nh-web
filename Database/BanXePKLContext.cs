using BanXePKL_Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace BanXePKL_Project.Database
{
    public class BanXePKLContext : DbContext
    {
        public BanXePKLContext(DbContextOptions<BanXePKLContext> options) : base(options)
        {
        }

        public DbSet<VaiTro> VaiTro { get; set; } = null!;
        public DbSet<NguoiDung> NguoiDung { get; set; } = null!;
        public DbSet<HangXe> HangXe { get; set; } = null!;
        public DbSet<DanhMuc> DanhMuc { get; set; } = null!;
        public DbSet<Xe> Xe { get; set; } = null!;
        public DbSet<AnhXe> AnhXe { get; set; } = null!;
        public DbSet<GioHang> GioHang { get; set; } = null!;
        public DbSet<GioHangChiTiet> GioHangChiTiet { get; set; } = null!;
        public DbSet<DonHang> DonHang { get; set; } = null!;
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; } = null!;
        public DbSet<DanhGia> DanhGia { get; set; } = null!;
        public DbSet<KhuyenMai> KhuyenMai { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== NguoiDung =====
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.Property(e => e.VaiTroId).HasDefaultValue(2);
                entity.Property(e => e.TrangThai).HasDefaultValue(true);
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.VaiTro)
                    .WithMany(v => v.NguoiDung)
                    .HasForeignKey(e => e.VaiTroId);
            });

            // ===== Xe =====
            modelBuilder.Entity<Xe>(entity =>
            {
                entity.Property(e => e.GiaBan).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TrangThai).HasDefaultValue(true);
                entity.Property(e => e.NgayThem).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.HangXe)
                    .WithMany(h => h.Xe)
                    .HasForeignKey(e => e.HangXeId);

                entity.HasOne(e => e.DanhMuc)
                    .WithMany(d => d.Xe)
                    .HasForeignKey(e => e.DanhMucId);
            });

            // ===== AnhXe =====
            modelBuilder.Entity<AnhXe>(entity =>
            {
                entity.HasOne(e => e.Xe)
                    .WithMany(x => x.AnhXe)
                    .HasForeignKey(e => e.XeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== GioHang =====
            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.NguoiDung)
                    .WithMany(n => n.GioHang)
                    .HasForeignKey(e => e.UserId);
            });

            // ===== GioHangChiTiet =====
            modelBuilder.Entity<GioHangChiTiet>(entity =>
            {
                entity.HasOne(e => e.GioHang)
                    .WithMany(g => g.GioHangChiTiet)
                    .HasForeignKey(e => e.GioHangId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Xe)
                    .WithMany(x => x.GioHangChiTiet)
                    .HasForeignKey(e => e.XeId);
            });

            // ===== DonHang =====
            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.Property(e => e.TongTien).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PhuongThucThanhToan).HasDefaultValue("COD");
                entity.Property(e => e.TrangThaiDonHang).HasDefaultValue("ChoXacNhan");
                entity.Property(e => e.NgayDat).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.NguoiDung)
                    .WithMany(n => n.DonHang)
                    .HasForeignKey(e => e.UserId);
            });

            // ===== ChiTietDonHang =====
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.Property(e => e.DonGia).HasColumnType("decimal(18,2)");
                // ThanhTien là cột tính toán (computed column) trong SQL: SoLuong * DonGia
                entity.Property(e => e.ThanhTien)
                    .HasColumnType("decimal(18,2)")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(e => e.DonHang)
                    .WithMany(d => d.ChiTietDonHang)
                    .HasForeignKey(e => e.DonHangId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Xe)
                    .WithMany(x => x.ChiTietDonHang)
                    .HasForeignKey(e => e.XeId);
            });

            // ===== DanhGia =====
            modelBuilder.Entity<DanhGia>(entity =>
            {
                entity.Property(e => e.NgayDanhGia).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Xe)
                    .WithMany(x => x.DanhGia)
                    .HasForeignKey(e => e.XeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.NguoiDung)
                    .WithMany(n => n.DanhGia)
                    .HasForeignKey(e => e.UserId);
            });

            // ===== KhuyenMai =====
            modelBuilder.Entity<KhuyenMai>(entity =>
            {
                entity.Property(e => e.TrangThai).HasDefaultValue(true);
                entity.HasIndex(e => e.MaCode).IsUnique();
            });

            // ===== VaiTro =====
            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.HasIndex(e => e.TenVaiTro).IsUnique();
            });

            // ===== HangXe =====
            modelBuilder.Entity<HangXe>(entity =>
            {
                entity.HasIndex(e => e.TenHang).IsUnique();
            });

            // ===== DanhMuc =====
            modelBuilder.Entity<DanhMuc>(entity =>
            {
                entity.HasIndex(e => e.TenDanhMuc).IsUnique();
            });
        }
    }
}
