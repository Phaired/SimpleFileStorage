using System.ComponentModel.DataAnnotations;

namespace FileStorage.Models
{
    public class File
    {
        [Key]
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public required string Hash { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

