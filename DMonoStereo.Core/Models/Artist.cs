using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMonoStereo.Core.Models
{
    /// <summary>
    /// Модель исполнителя для хранения в базе данных
    /// </summary>
    [Table("Artists")]
    public partial record Artist
    {
        /// <summary>
        /// Уникальный идентификатор исполнителя в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Название исполнителя (максимум 200 символов)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Обложка исполнителя в виде массива байтов (BLOB в SQLite)
        /// </summary>
        public byte[]? CoverImage { get; set; }

        /// <summary>
        /// Коллекция альбомов этого исполнителя
        /// </summary>
        public ICollection<Album> Albums { get; set; } = new List<Album>();

        /// <summary>
        /// Дата добавления исполнителя в библиотеку
        /// </summary>
        [Required]
        public DateTime DateAdded { get; set; }
    }
}