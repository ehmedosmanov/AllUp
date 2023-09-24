using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public float Tax { get; set; }
        public bool HasStock { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
