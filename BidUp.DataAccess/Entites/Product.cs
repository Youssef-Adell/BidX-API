namespace BidUp.DataAccess.Entites;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string ThumbnailUrl { get; set; } = null!; // i did not make it required because if i did i would be forced to provide a value for it while the mapping from CreateAuctionRequrest to Product using the mapper
    public string? Description { get; set; }
    public ProductCondition Condition { get; set; }
    public int AuctionId { get; set; }

    public ICollection<ProductImage> Images { get; } = new List<ProductImage>();
}
