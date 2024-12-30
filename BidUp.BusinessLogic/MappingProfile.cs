using AutoMapper;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;
using BidUp.DataAccess.Entites;

namespace BidUp.BusinessLogic;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //---Categories & Cities---
        CreateMap<City, CityResponse>();

        CreateMap<Category, CategoryResponse>();

        //---Auctions---
        CreateMap<CreateAuctionRequest, Auction>()
            .AfterMap((s, d) => d.Product = new()
            {
                Name = s.ProductName,
                Condition = s.ProductCondition,
                Description = s.ProductDescription
            });

        CreateMap<Auction, AuctionResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductCondition, o => o.MapFrom(s => s.Product.Condition))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Product.ThumbnailUrl));


        CreateMap<Auction, AuctionDetailsResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductDescription, o => o.MapFrom(s => s.Product.Description))
            .ForMember(d => d.ProductCondition, o => o.MapFrom(s => s.Product.Condition))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category!.Name))
            .ForMember(d => d.City, o => o.MapFrom(s => s.City!.Name))
            .ForMember(d => d.Auctioneer, o => o.MapFrom(s => new Auctioneer
            {
                Id = s.AuctioneerId,
                Name = string.Concat(s.Auctioneer!.FirstName, " ", s.Auctioneer.LastName),
                ProfilePictureUrl = s.Auctioneer.ProfilePictureUrl,
                TotalRating = s.Auctioneer.TotalRating
            }))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Product.Images.Select(i => i.Url)));

        //---Bids---
        CreateMap<BidRequest, Bid>();
        CreateMap<Bid, BidResponse>()
            .ForMember(d => d.Bidder, o => o.MapFrom(s => new Bidder
            {
                Id = s.Bidder!.Id,
                Name = string.Concat(s.Bidder!.FirstName, " ", s.Bidder.LastName),
                ProfilePictureUrl = s.Bidder.ProfilePictureUrl,
                TotalRating = s.Bidder.TotalRating
            }));

        //---Messages---
        CreateMap<Message, MessageResponse>();

        //---Reviews---
        CreateMap<Review, MyReviewResponse>();
    }
}