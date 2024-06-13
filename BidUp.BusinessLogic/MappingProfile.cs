using AutoMapper;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.DataAccess.Entites;

namespace BidUp.BusinessLogic;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<City, CityResponse>();

        CreateMap<Category, CategoryResponse>();

        CreateMap<CreateAuctionRequest, Auction>()
            .AfterMap((s, d) => d.Product = new()
            {
                Name = s.ProductName,
                Condition = s.ProductCondition,
                Description = s.ProductDescription
            });

        CreateMap<Auction, AuctionResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s => s.HighestBid != null ? s.HighestBid.Amount : s.StartingPrice))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Product.ThumbnailUrl));
    }
}