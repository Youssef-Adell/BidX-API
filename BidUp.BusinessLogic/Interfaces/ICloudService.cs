using BidUp.BusinessLogic.DTOs.CloudDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICloudService
{
    Task<AppResult<UploadResponse>> UploadSvgIcon(Stream icon);

    /// <summary>
    /// Upload and transform the image to a thumbnail.
    /// </summary>
    Task<AppResult<UploadResponse>> UploadThumbnail(Stream image);
}
