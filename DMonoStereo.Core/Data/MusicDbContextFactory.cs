using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DMonoStereo.Core.Data
{
    /// <summary>
    /// Фабрика для создания контекста базы данных во время разработки
    /// </summary>
    public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDbContext>
    {
        /// <summary>
        /// Создать контекст базы данных для миграций
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Контекст базы данных</returns>
        public MusicDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MusicDbContext>();

            // Используем временную базу данных для миграций
            // Явно указываем сборку с миграциями
            optionsBuilder.UseSqlite(
                "Data Source=music.db",
                sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(MusicDbContext).Assembly.GetName().Name));

            return new MusicDbContext(optionsBuilder.Options);
        }
    }
}