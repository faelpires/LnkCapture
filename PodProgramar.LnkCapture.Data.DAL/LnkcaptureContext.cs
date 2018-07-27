using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PodProgramar.LnkCapture.Data.Models;

namespace PodProgramar.LnkCapture.Data.DAL
{
    public partial class LnkCaptureContext : DbContext
    {
        public LnkCaptureContext(DbContextOptions<LnkCaptureContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<CultureInfo> CultureInfo { get; set; }
        public virtual DbSet<Link> Link { get; set; }
        public virtual DbSet<LinkReader> LinkReader { get; set; }
        public virtual DbSet<LinkReaderLog> LinkReaderLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Config>(entity =>
            {
                entity.Property(e => e.ConfigId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Culture)
                    .WithMany(p => p.Config)
                    .HasForeignKey(d => d.CultureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Config_CultureInfo");
            });

            modelBuilder.Entity<CultureInfo>(entity =>
            {
                entity.HasKey(e => e.CultureId);

                entity.Property(e => e.CultureId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Culture)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Link>(entity =>
            {
                entity.Property(e => e.LinkId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .IsUnicode(false);

                entity.Property(e => e.Keywords)
                    .IsUnicode(false);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ThumbnailUri)
                    .HasMaxLength(2048)
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

            modelBuilder.Entity<LinkReader>(entity =>
            {
                entity.HasIndex(e => new { e.ChatId, e.UserId })
                    .HasName("IX_LinkReader")
                    .IsUnique();

                entity.Property(e => e.LinkReaderId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<LinkReaderLog>(entity =>
            {
                entity.Property(e => e.LinkReaderLogId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccessDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.LinkReader)
                    .WithMany(p => p.LinkReaderLog)
                    .HasForeignKey(d => d.LinkReaderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LinkReaderLog_LinkReader");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
