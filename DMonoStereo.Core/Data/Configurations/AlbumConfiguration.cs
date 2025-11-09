using DMonoStereo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMonoStereo.Core.Data.Configurations
{
	/// <summary>
	/// Конфигурация сущности альбома для EF Core
	/// </summary>
	public class AlbumConfiguration : IEntityTypeConfiguration<Album>
	{
		/// <summary>
		/// Настройка сущности альбома
		/// </summary>
		public void Configure(EntityTypeBuilder<Album> builder)
		{
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id)
				.ValueGeneratedOnAdd();

			builder.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(e => e.DateAdded)
				.IsRequired();

			builder.Property(e => e.CoverImage)
				.HasColumnType("BLOB");

			builder.HasOne(e => e.Artist)
				.WithMany(e => e.Albums)
				.HasForeignKey(e => e.ArtistId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(e => e.Tracks)
				.WithOne(e => e.Album)
				.HasForeignKey(e => e.AlbumId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(e => e.Name)
				.HasDatabaseName("IX_Albums_Name");

			builder.HasIndex(e => e.ArtistId)
				.HasDatabaseName("IX_Albums_ArtistId");

			builder.HasIndex(e => e.DateAdded)
				.HasDatabaseName("IX_Albums_DateAdded");

			builder.HasIndex(e => e.Year)
				.HasDatabaseName("IX_Albums_Year");
		}
	}
}

