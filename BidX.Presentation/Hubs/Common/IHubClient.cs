using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.Presentation.Hubs;

public partial interface IHubClient
{
    /// <summary>
    /// Triggerd for the caller client only if there is an error
    /// </summary>
    Task ErrorOccurred(ErrorResponse error);
}
