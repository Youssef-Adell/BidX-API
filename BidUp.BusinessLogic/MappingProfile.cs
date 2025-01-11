using System.Diagnostics;
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
        #region Auctions
        CreateMap<Auction, AuctionResponse>()
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s =>
                 s.Bids!.OrderByDescending(b => b.Amount)
                 .Select(b => (decimal?)b.Amount)
                 .FirstOrDefault() ?? s.StartingPrice));

        CreateMap<Auction, AuctionUserHasBidOnResponse>()
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s =>
                 s.Bids!.OrderByDescending(b => b.Amount)
                 .Select(b => (decimal?)b.Amount)
                 .FirstOrDefault() ?? s.StartingPrice))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.EndTime > DateTimeOffset.UtcNow));

        CreateMap<User, Auctioneer>();
        CreateMap<Auction, AuctionDetailsResponse>()
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s =>
                 s.Bids!.OrderByDescending(b => b.Amount)
                 .Select(b => (decimal?)b.Amount)
                 .FirstOrDefault() ?? s.StartingPrice))
            .ForMember(d => d.Auctioneer, o => o.MapFrom(s => s.Auctioneer))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category!.Name))
            .ForMember(d => d.City, o => o.MapFrom(s => s.City!.Name))
            .ForMember(d => d.ProductImages, o => o.MapFrom(s => s.ProductImages!.Select(i => i.Url)));

        CreateMap<CreateAuctionRequest, Auction>()
            .ForMember(d => d.AuctioneerId, o => o.MapFrom((_, _, _, context) => (int)context.Items["AuctioneerId"]))
            .ForMember(d => d.StartTime, o => o.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(d => d.EndTime, o => o.MapFrom(s => DateTimeOffset.UtcNow.AddSeconds(s.DurationInSeconds)));
        #endregion


        #region Bids
        CreateMap<User, Bidder>();
        CreateMap<Bid, BidResponse>();

        CreateMap<BidRequest, Bid>()
            .ForMember(d => d.BidderId, o => o.MapFrom((_, _, _, context) => (int)context.Items["BidderId"]));
        #endregion


        #region Cities
        CreateMap<City, CityResponse>();
        #endregion


        #region Categories
        CreateMap<Category, CategoryResponse>();
        #endregion


        #region Chats
        CreateMap<Message, MessageResponse>();
        #endregion


        #region Reviews
        CreateMap<Review, MyReviewResponse>();
        #endregion
    }
}