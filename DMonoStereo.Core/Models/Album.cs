using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMonoStereo.Core.Models
{
    /// <summary>
    /// Модель альбома для хранения в базе данных
    /// </summary>
    [Table("Albums")]
    public record Album
    {
        /// <summary>
        /// Уникальный идентификатор альбома в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Название альбома (максимум 200 символов)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Год выпуска альбома
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Рейтинг альбома (1-5)
        /// </summary>
        [Range(1, 5)]
        public int? Rating { get; set; }

        /// <summary>
        /// Обложка альбома в виде массива байтов (BLOB в SQLite)
        /// </summary>
        public byte[]? CoverImage { get; set; }

        /// <summary>
        /// Идентификатор исполнителя (внешний ключ)
        /// </summary>
        [Required]
        public int ArtistId { get; set; }

        /// <summary>
        /// Исполнитель альбома
        /// </summary>
        [ForeignKey(nameof(ArtistId))]
        public Artist Artist { get; set; } = null!;

        /// <summary>
        /// Коллекция треков альбома
        /// </summary>
        public ICollection<Track> Tracks { get; set; } = new List<Track>();

        /// <summary>
        /// Дата добавления альбома в библиотеку
        /// </summary>
        [Required]
        public DateTime DateAdded { get; set; }
    }
}