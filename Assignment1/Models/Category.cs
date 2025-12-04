using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}