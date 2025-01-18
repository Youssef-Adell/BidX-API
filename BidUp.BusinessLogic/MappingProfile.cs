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

        CreateMap<AddCategoryRequest, Category>()
            .ForMember(d => d.IconUrl, o => o.MapFrom((_, _, _, context) => (int)context.Items["IconUrl"]));
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


        #region Chats
        int? userId = null;
        CreateMap<Chat, ChatDetailsResponse>()
                    .ForMember(dest => dest.ParticipantId, opt => opt.MapFrom(src =>
                        src.Participant1Id == userId ? src.Participant2!.Id : src.Participant1!.Id))
                    .ForMember(dest => dest.ParticipantName, opt => opt.MapFrom(src =>
                        src.Participant1Id == userId ? src.Participant2!.FullName : src.Participant1!.FullName))
                    .ForMember(dest => dest.ParticipantProfilePictureUrl, opt => opt.MapFrom(src =>
                        src.Participant1Id == userId ? src.Participant2!.ProfilePictureUrl : src.Participant1!.ProfilePictureUrl))
                    .ForMember(dest => dest.IsParticipantOnline, opt => opt.MapFrom(src =>
                        src.Participant1Id == userId ? src.Participant2!.IsOnline : src.Participant1!.IsOnline))
                    .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.LastMessage!.Content))
                    .ForMember(dest => dest.UnreadMessagesCount, opt => opt.MapFrom(src =>
                        src.Messages!.Count(m => m.RecipientId == userId && !m.IsRead)));
        CreateMap<CreateChatRequest, Chat>()
            .ForMember(d => d.Participant1Id, o => o.MapFrom((_, _, _, context) => (int)context.Items["Participant1Id"]))
            .ForMember(d => d.Participant2Id, o => o.MapFrom(s => s.ParticipantId))
            .ForMember(d => d.Participant2, o => o.MapFrom((_, _, _, context) => (User)context.Items["Participant2"]));

        CreateMap<Chat, ChatSummeryResponse>()
            .ForMember(d => d.ParticipantId, o => o.MapFrom((s, _, _, context) =>
                s.Participant1Id == (int)context.Items["UserId"] ? s.Participant2Id : s.Participant1Id))
            .ForMember(d => d.ParticipantName, o => o.MapFrom((s, _, _, context) =>
                s.Participant1Id == (int)context.Items["UserId"] ? s.Participant2!.FullName : s.Participant1!.FullName))
            .ForMember(d => d.ParticipantProfilePictureUrl, o => o.MapFrom((s, _, _, context) =>
                s.Participant1Id == (int)context.Items["UserId"] ? s.Participant2!.ProfilePictureUrl : s.Participant1!.ProfilePictureUrl))
            .ForMember(d => d.IsParticipantOnline, o => o.MapFrom((s, _, _, context) =>
                s.Participant1Id == (int)context.Items["UserId"] ? s.Participant2!.IsOnline : s.Participant1!.IsOnline));
        #endregion


        #region Messages
        CreateMap<Message, MessageResponse>();
        #endregion
    }
}