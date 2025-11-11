using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMonoStereo.Core.Models
{
    /// <summary>
    /// Модель трека для хранения в базе данных
    /// </summary>
    [Table("Tracks")]
    public record Track
    {
        /// <summary>
        /// Уникальный идентификатор трека в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Название трека (максимум 200 символов)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Продолжительность трека в секундах
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Продолжительность должна быть больше 0")]
        public int Duration { get; set; }

        /// <summary>
        /// Рейтинг трека (1-5)
        /// </summary>
        [Range(1, 5)]
        public int? Rating { get; set; }

        /// <summary>
        /// Номер трека в альбоме
        /// </summary>
        public int? TrackNumber { get; set; }

        /// <summary>
        /// Идентификатор альбома (внешний ключ)
        /// </summary>
        [Required]
        public int AlbumId { get; set; }

        /// <summary>
        /// Альбом трека
        /// </summary>
        [ForeignKey(nameof(AlbumId))]
        public Album Album { get; set; } = null!;
    }
}