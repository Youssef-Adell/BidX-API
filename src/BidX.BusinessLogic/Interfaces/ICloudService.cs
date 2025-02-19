using BidX.BusinessLogic.DTOs.CloudDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface ICloudService
{
    Task<Result<UploadResponse>> UploadSvgIcon(Stream icon);

    /// <summary>
    /// Upload and transform the image to a thumbnail.
    /// </summary>
    Task<Result<UploadResponse>> UploadThumbnail(Stream image);

    Task<Result<UploadResponse[]>> UploadImages(IEnumerable<Stream> images);
}
