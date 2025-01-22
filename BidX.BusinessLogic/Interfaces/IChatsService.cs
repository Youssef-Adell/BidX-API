using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IChatsService
{
    Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams);
    Task<Result<ChatSummeryResponse>> CreateChatOrGetIfExist(int callerId, CreateChatRequest request);
    Task<Result<Page<MessageResponse>>> GetChatMessages(int callerId, int chatId, MessagesQueryParams queryParams);
    Task<Result<MessageResponse>> SendMessage(int senderId, SendMessageRequest request);
    Task<IEnumerable<int>> ChangeUserStatus(int userId, bool isOnline);
    Task<bool> HasUnreadMessages(int userId);
}
