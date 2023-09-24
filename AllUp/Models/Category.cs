using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models
{
    public class Category
    {
        //self join//
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Img { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
        public bool IsDeactive { get; set; }
        public bool IsMain { get; set; }
        public List<Category>? Children { get; set; } //bir nece child-i var Bottoms Tops & Sets ve s.
        public List<ProductCategory>? ProductCategories { get; set; } // many to many 
        public Category? Parent { get; set; }   
        public int? ParentId { get; set; } 
    }
}
