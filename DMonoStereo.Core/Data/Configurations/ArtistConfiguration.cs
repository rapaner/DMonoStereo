using DMonoStereo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMonoStereo.Core.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности исполнителя для EF Core
    /// </summary>
    public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
    {
        /// <summary>
        /// Настройка сущности исполнителя
        /// </summary>
        public void Configure(EntityTypeBuilder<Artist> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.CoverImage)
                .HasColumnType("BLOB");

            builder.Property(e => e.DateAdded)
                .IsRequired();

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Artists_Name");

            builder.HasIndex(e => e.DateAdded)
                .HasDatabaseName("IX_Artists_DateAdded");
        }
    }
}