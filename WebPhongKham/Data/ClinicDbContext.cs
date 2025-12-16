using Microsoft.EntityFrameworkCore;
using WebPhongKham.Models;

namespace WebPhongKham.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }
        public DbSet<BacSi_NgayNghi> BacSi_NgayNghis { get; set; }
        public DbSet<BacSi_LichLamViec> BacSi_LichLamViecs { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<SMS_Log> SMS_Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BacSi>()
                .ToTable("BacSi", tb =>
                {
                    tb.HasTrigger("TRG_BacSi");
                    tb.UseSqlOutputClause(false);
                })
                .HasKey(x => x.IdBacSi);

            modelBuilder.Entity<BenhNhan>().ToTable("BenhNhan").HasKey(x => x.IdBenhNhan);
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan").HasKey(x => x.Id);
            modelBuilder.Entity<ThongBao>().ToTable("ThongBao").HasKey(x => x.IdThongBao);
            modelBuilder.Entity<TinTuc>().ToTable("TinTuc").HasKey(x => x.Id);

            modelBuilder.Entity<BacSi_NgayNghi>().ToTable("BacSi_NgayNghi").HasKey(x => x.Id);
            modelBuilder.Entity<BacSi_LichLamViec>().ToTable("BacSi_LichLamViec").HasKey(x => x.Id);

            modelBuilder.Entity<LichHen>()
                .ToTable("LichHen", tb => tb.HasTrigger("TRG_LichHen"))
                .HasKey(x => x.IdLichHen);
            modelBuilder.Entity<LichHen>()
                .HasOne(x => x.BenhNhan)
                .WithMany()
                .HasForeignKey(x => x.IdBenhNhan);

            modelBuilder.Entity<SMS_Log>().ToTable("SMS_Log").HasKey(x => x.Id);
        }
    }
}
