using AutoMapper;
using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.CategoryDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CityDTOs;
using BidX.BusinessLogic.DTOs.ReviewsDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Auctions
        CreateMap<Auction, AuctionResponse>()
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s =>
                 s.Bids!.OrderByDescending(b => b.Amount)
                 .Select(b => (decimal?)b.Amount)
                 .FirstOrDefault() ?? s.StartingPrice))
            .AfterMap((s, d) => d.CurrentPrice = d.CurrentPrice == 0 ? s.StartingPrice : d.CurrentPrice); // To ensure that CurrentPrice calculated in case of mqpping with Map<>() not ProjectTo<>()

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
            .ForMember(d => d.Participant2Id, o => o.MapFrom(s => s.ParticipantId));

        int? UserId = null;
        CreateMap<Chat, ChatSummeryResponse>()
            .ForMember(d => d.ParticipantId, o => o.MapFrom(s =>
                s.Participant1Id == UserId ? s.Participant2Id : s.Participant1Id))
            .ForMember(d => d.ParticipantName, o => o.MapFrom(s =>
                s.Participant1Id == UserId ? s.Participant2!.FullName : s.Participant1!.FullName))
            .ForMember(d => d.ParticipantProfilePictureUrl, o => o.MapFrom(s =>
                s.Participant1Id == UserId ? s.Participant2!.ProfilePictureUrl : s.Participant1!.ProfilePictureUrl))
            .ForMember(d => d.IsParticipantOnline, o => o.MapFrom(s =>
                s.Participant1Id == UserId ? s.Participant2!.IsOnline : s.Participant1!.IsOnline));
        #endregion


        #region Messages
        CreateMap<Message, MessageResponse>();
        CreateMap<SendMessageRequest, Message>()
            .ForMember(d => d.Content, o => o.MapFrom(s => s.Message))
              .ForMember(d => d.SenderId, o => o.MapFrom((_, _, _, context) => (int)context.Items["SenderId"]))
              .ForMember(d => d.RecipientId, o => o.MapFrom((_, _, _, context) => (int)context.Items["RecipientId"]));
        #endregion
    }
}