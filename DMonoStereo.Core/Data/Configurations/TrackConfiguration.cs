using DMonoStereo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMonoStereo.Core.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности трека для EF Core
    /// </summary>
    public class TrackConfiguration : IEntityTypeConfiguration<Track>
    {
        /// <summary>
        /// Настройка сущности трека
        /// </summary>
        public void Configure(EntityTypeBuilder<Track> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Duration)
                .IsRequired();

            builder.HasOne(e => e.Album)
                .WithMany(e => e.Tracks)
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Tracks_Name");

            builder.HasIndex(e => e.AlbumId)
                .HasDatabaseName("IX_Tracks_AlbumId");

            builder.HasIndex(e => e.TrackNumber)
                .HasDatabaseName("IX_Tracks_TrackNumber");
        }
    }
}