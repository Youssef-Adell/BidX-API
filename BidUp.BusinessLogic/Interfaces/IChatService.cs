using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IChatService
{
    Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams);
    Task<AppResult<ChatSummeryResponse>> IntiateChat(int senderId, int receiverId);
    Task<AppResult<MessageResponse>> SendMessage(int senderId, MessageRequest messageRequest);
    Task<AppResult> MarkReceivedMessagesAsSeen(int userId, int chatId);
    Task<IEnumerable<int>> ChangeUserStatus(int userId, bool isOnline);
    Task<bool> HasUnseenMessages(int userId);
}
