using System.Net;
using System.Text;
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
    private const string ThumbnailCropMode = "fill";
    private const string ProductImageCropMode = "fit";
    private readonly (int Width, int Height) thumbnailSize;
    private readonly (int Width, int Height) productImageSize;
    private readonly int maxIconSizeAllowed;
    private readonly int maxImageSizeAllowed;
    private readonly Cloudinary cloudinary;
    private readonly ILogger<CloudinaryCloudService> logger;

    public CloudinaryCloudService(ILogger<CloudinaryCloudService> logger, IConfiguration configuration)
    {
        this.logger = logger;

        cloudinary = new Cloudinary(new Account(
            cloud: Environment.GetEnvironmentVariable("CLOUDINARY_CLOUDNAME"),
            apiKey: Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY"),
            apiSecret: Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET")
        ));
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

        if (!int.TryParse(configuration["images:ProductImageWidth"], out productImageSize.Width) || !int.TryParse(configuration["images:ProductImageHeight"], out productImageSize.Height))
        {
            productImageSize.Width = 800; //800 PX
            productImageSize.Height = 800; //800 PX
        }
    }

    public async Task<AppResult<UploadResponse>> UploadSvgIcon(Stream icon)
    {
        var validationResult = ValidateIcon(icon);

        if (!validationResult.Succeeded)
            return AppResult<UploadResponse>.Failure(validationResult.Error!);

        var response = await UploadIcon(icon);

        return AppResult<UploadResponse>.Success(response);
    }

    public async Task<AppResult<UploadResponse>> UploadThumbnail(Stream image)
    {
        var validationResult = ValidateImage(image);

        if (!validationResult.Succeeded)
            return AppResult<UploadResponse>.Failure(validationResult.Error!);

        var response = await UploadImage(image, thumbnailSize, ThumbnailCropMode);

        return AppResult<UploadResponse>.Success(response);
    }

    public async Task<AppResult<UploadResponse[]>> UploadImages(IEnumerable<Stream> images)
    {
        foreach (var image in images)
        {
            var validationResult = ValidateImage(image);
            if (!validationResult.Succeeded)
                return AppResult<UploadResponse[]>.Failure(validationResult.Error!);
        }

        var uploadTasks = images.Select(image => UploadImage(image, productImageSize, ProductImageCropMode));

        var response = await Task.WhenAll(uploadTasks);

        return AppResult<UploadResponse[]>.Success(response);
    }


    private async Task<UploadResponse> UploadIcon(Stream icon)
    {
        var imageId = Guid.NewGuid();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageId.ToString(), icon),
            PublicId = imageId.ToString()
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Upload failed for an svg icon: {uploadResult.Error.Message}");

        var response = new UploadResponse
        {
            FileId = imageId,
            FileUrl = uploadResult.SecureUrl.ToString(),
        };

        return response;
    }

    private async Task<UploadResponse> UploadImage(Stream image, (int Width, int Height) imageSize, string cropMode)
    {
        var imageId = Guid.NewGuid();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageId.ToString(), image),
            PublicId = imageId.ToString(),
            Transformation = new Transformation().Width(imageSize.Width).Height(imageSize.Height).Crop(cropMode).Quality("auto"),
            Format = "jpg"
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Upload failed for an image: {uploadResult.Error.Message}");

        var response = new UploadResponse
        {
            FileId = imageId,
            FileUrl = uploadResult.SecureUrl.ToString(),
        };

        return response;
    }

    private AppResult ValidateIcon(Stream icon)
    {
        if (icon.Length > maxIconSizeAllowed || icon.Length <= 0)
            return AppResult.Failure(ErrorCode.UPLOADED_FILE_INVALID, [$"The icon size must not exceed {maxIconSizeAllowed / 1024} KB."]);

        if (!IsSvgFile(icon))
            return AppResult.Failure(ErrorCode.UPLOADED_FILE_INVALID, ["The only icon format supported is SVG."]);

        return AppResult.Success();
    }

    private AppResult ValidateImage(Stream image)
    {
        if (image.Length > maxImageSizeAllowed || image.Length <= 0)
            return AppResult.Failure(ErrorCode.UPLOADED_FILE_INVALID, [$"There is an image exceeds the maximum size limit of {maxImageSizeAllowed / 1024} KB."]);

        if (!IsImageFile(image))
            return AppResult.Failure(ErrorCode.UPLOADED_FILE_INVALID, ["There is an image in an invalid format."]);

        return AppResult.Success();
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
