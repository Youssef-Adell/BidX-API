using System.Net;
using System.Xml.Linq;
using BidUp.BusinessLogic.DTOs.CloudDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FileTypeChecker.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BidUp.BusinessLogic.Services;

public class CloudinaryCloudService : ICloudService
{
    private readonly ILogger<CloudinaryCloudService> logger;
    private readonly Cloudinary cloudinary;
    private readonly int maxIconSizeAllowed;
    private readonly int maxImageSizeAllowed;
    private readonly (int Width, int Height) thumbnailSize;

    public CloudinaryCloudService(ILogger<CloudinaryCloudService> logger, IConfiguration configuration)
    {
        this.logger = logger;

        cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
        cloudinary.Api.Secure = true;

        if (!int.TryParse(configuration["images:MaxIconSizeAllowed"], out maxIconSizeAllowed))
            maxIconSizeAllowed = 256 * 1024; //256 KB

        if (!int.TryParse(configuration["images:MaxImageSizeAllowed"], out maxImageSizeAllowed))
            maxImageSizeAllowed = 1 * 1024 * 1024; //1 MB

        if (!int.TryParse(configuration["images:ThumbnailWidth"], out thumbnailSize.Width) || !int.TryParse(configuration["images:ThumbnailHeight"], out thumbnailSize.Height))
        {
            thumbnailSize.Width = 200; //200 PX
            thumbnailSize.Height = 200; //200 PX
        }
    }

    public async Task<AppResult<UploadResponse>> UploadSvgIcon(Stream icon)
    {
        if (icon.Length > maxIconSizeAllowed || icon.Length <= 0)
            return AppResult<UploadResponse>.Failure(ErrorCode.UPLOADED_FILE_INVALID, [$"The icon size must not exceed {maxIconSizeAllowed / 1024} KB."]);

        if (!IsSvgFile(icon))
            return AppResult<UploadResponse>.Failure(ErrorCode.UPLOADED_FILE_INVALID, ["The only icon format supported is SVG."]);

        var imageId = Guid.NewGuid();
        var uploadParams = new ImageUploadParams { File = new FileDescription(imageId.ToString(), icon), PublicId = imageId.ToString() };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Upload failed for an svg icon: {uploadResult.Error.Message}");

        var response = new UploadResponse
        {
            FileId = uploadResult.PublicId,
            FileUrl = uploadResult.SecureUrl.ToString(),
        };

        return AppResult<UploadResponse>.Success(response);
    }

    public async Task<AppResult<UploadResponse>> UploadThumbnail(Stream image)
    {
        if (image.Length > maxImageSizeAllowed || image.Length <= 0)
            return AppResult<UploadResponse>.Failure(ErrorCode.UPLOADED_FILE_INVALID, [$"The profile picture size must not exceed {maxImageSizeAllowed / 1024} KB."]);

        if (!IsImageFile(image))
            return AppResult<UploadResponse>.Failure(ErrorCode.UPLOADED_FILE_INVALID, ["Invalid image format."]);

        var imageId = Guid.NewGuid();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageId.ToString(), image),
            PublicId = imageId.ToString(),
            Transformation = new Transformation().Width(thumbnailSize.Width).Height(thumbnailSize.Height).Crop("fill").Quality("auto"),
            Format = "jpg"
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Upload failed for a profile image: {uploadResult.Error.Message}");

        var response = new UploadResponse
        {
            FileId = uploadResult.PublicId,
            FileUrl = uploadResult.SecureUrl.ToString(),
        };

        return AppResult<UploadResponse>.Success(response);
    }


    private static bool IsSvgFile(Stream stream)
    {
        try
        {
            stream.Position = 0;
            var doc = XDocument.Load(stream); // Try to load the stream as XML
            stream.Position = 0; // Reset stream position again for further processing if needed

            // Ensure it contains <svg> root element
            return doc.Root?.Name.LocalName == "svg";
        }
        catch
        {
            // If loading as XML fails, it's not a valid SVG
            stream.Position = 0;
            return false;
        }
    }

    private static bool IsImageFile(Stream stream)
    {
        stream.Position = 0;

        var isImage = stream.IsImage();

        // Reset stream position again for further processing otherwise cloudinary will response with "invalid image type." error
        stream.Position = 0;

        return isImage;
    }
}
