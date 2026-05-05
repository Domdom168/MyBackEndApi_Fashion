using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Column("name_khmer")]
        public string? NameKhmer { get; set; }

        [Column("name_english")]
        public string? NameEnglish { get; set; }
        [Column("icon")]
        public string? Icon { get; set; }
        public string? Color { get; set; }

        public DateTime created_at { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
