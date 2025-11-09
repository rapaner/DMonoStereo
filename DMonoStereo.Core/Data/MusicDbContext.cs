using DMonoStereo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DMonoStereo.Core.Data
{
	/// <summary>
	/// Контекст базы данных для музыкальной библиотеки
	/// </summary>
	public class MusicDbContext : DbContext
	{
		/// <summary>
		/// Коллекция исполнителей в базе данных
		/// </summary>
		public DbSet<Artist> Artists { get; set; }

		/// <summary>
		/// Коллекция альбомов в базе данных
		/// </summary>
		public DbSet<Album> Albums { get; set; }

		/// <summary>
		/// Коллекция треков в базе данных
		/// </summary>
		public DbSet<Track> Tracks { get; set; }

		/// <summary>
		/// Конструктор контекста базы данных
		/// </summary>
		/// <param name="options">Опции для настройки контекста</param>
		public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options)
		{
		}

		/// <summary>
		/// Настройка модели данных при создании контекста
		/// </summary>
		/// <param name="modelBuilder">Построитель модели</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(MusicDbContext).Assembly);
		}
	}
}



