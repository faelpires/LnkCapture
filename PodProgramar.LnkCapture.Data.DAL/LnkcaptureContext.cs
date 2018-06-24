using Microsoft.EntityFrameworkCore;
using PodProgramar.LnkCapture.Data.Models;

namespace PodProgramar.LnkCapture.Data.DAL
{
    public partial class LnkCaptureContext : DbContext
    {
        public LnkCaptureContext(DbContextOptions<LnkCaptureContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Link> Link { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Link>(entity =>
            {
                entity.Property(e => e.LinkId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Uri)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}