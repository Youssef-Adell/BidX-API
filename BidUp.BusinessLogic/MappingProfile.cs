using System.Diagnostics;
using AutoMapper;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;
using BidUp.BusinessLogic.DTOs.ProfileDTOs;
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
        CreateMap<User, Reviewer>();
        CreateMap<Review, ReviewResponse>();

        CreateMap<Review, MyReviewResponse>();

        CreateMap<AddReviewRequest, Review>()
            .ForMember(d => d.ReviewerId, o => o.MapFrom((_, _, _, context) => (int)context.Items["ReviewerId"]))
            .ForMember(d => d.RevieweeId, o => o.MapFrom((_, _, _, context) => (int)context.Items["RevieweeId"]));
        #endregion

        #region  Profiles
        CreateMap<User, ProfileResponse>();

        #endregion


        #region Auth
        CreateMap<RegisterRequest, User>()
            .ForMember(d => d.UserName, o => o.MapFrom((_, _, _, context) => (string)context.Items["UserName"]))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim()));

        CreateMap<User, UserInfo>()
            .ForMember(d => d.Role, o => o.MapFrom((_, _, _, context) => (string)context.Items["Role"]));
        CreateMap<User, LoginResponse>()
            .ForMember(d => d.AccessToken, o => o.MapFrom((_, _, _, context) => (string)context.Items["AccessToken"]))
            .ForMember(d => d.User, o => o.MapFrom(s => s)); // Need to be specified explicitly otherwise the d.User will be null
        #endregion    
    }
}