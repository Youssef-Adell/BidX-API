using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IChatsService
{
    Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams);
    Task<AppResult<ChatSummeryResponse>> CreateChatOrGetIfExist(int callerId, CreateChatRequest request);
    Task<AppResult<Page<MessageResponse>>> GetChatMessages(int callerId, int chatId, MessagesQueryParams queryParams);
    Task<AppResult<MessageResponse>> SendMessage(int senderId, SendMessageRequest request);
    Task<IEnumerable<int>> ChangeUserStatus(int userId, bool isOnline);
    Task<bool> HasUnreadMessages(int userId);
}
