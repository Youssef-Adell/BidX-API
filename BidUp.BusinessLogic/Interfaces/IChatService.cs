using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IChatService
{
    Task<AppResult<ChatSummeryResponse>> IntiateChat(int senderId, int receiverId);
    Task<AppResult<MessageResponse>> SendMessage(int senderId, MessageRequest messageRequest);
}
