namespace AllUp.Models
{
    public class TagProduct
    {
        public int Id { get; set; }

        public Product Product { get; set; }
        public int ProductId { get; set; }

        public Tag Tag { get; set; }
        public int TagId { get; set; }
    }
}
