using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IChatsService
{
    Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams);
    Task<Result<ChatSummeryResponse>> CreateChatOrGetIfExist(int callerId, CreateChatRequest request);
    Task<Result<ChatSummeryResponse>> GetChat(int callerId, int chatId);
    Task<Result<Page<MessageResponse>>> GetChatMessages(int callerId, int chatId, MessagesQueryParams queryParams);
    Task SendMessage(int senderId, SendMessageRequest request);
    Task MarkMessageAsRead(int readerId, MarkMessageAsReadRequest request);
    Task NotifyUserWithUnreadChatsCount(int userId);
    Task NotifyParticipantsWithUserStatus(int userId, bool isOnline);
}
