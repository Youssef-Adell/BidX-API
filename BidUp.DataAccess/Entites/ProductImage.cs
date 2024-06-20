namespace BidUp.DataAccess.Entites;

public class ProductImage
{
    public Guid Id { get; set; } // i use Guid not int because i will use the PublicId of the image in cloudinary which is Guid as an Id for the image in the db
    public required string Url { get; set; }
    public int ProductId { get; set; }
}
