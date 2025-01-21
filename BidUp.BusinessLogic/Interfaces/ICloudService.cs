using BidUp.BusinessLogic.DTOs.CloudDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICloudService
{
    Task<Result<UploadResponse>> UploadSvgIcon(Stream icon);

    /// <summary>
    /// Upload and transform the image to a thumbnail.
    /// </summary>
    Task<Result<UploadResponse>> UploadThumbnail(Stream image);

    Task<Result<UploadResponse[]>> UploadImages(IEnumerable<Stream> images);
}
