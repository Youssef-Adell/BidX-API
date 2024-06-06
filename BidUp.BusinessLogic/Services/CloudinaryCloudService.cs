using System.Net;
using System.Xml.Linq;
using BidUp.BusinessLogic.DTOs.CloudDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BidUp.BusinessLogic.Services;

public class CloudinaryCloudService : ICloudService
{
    private readonly ILogger<CloudinaryCloudService> logger;
    private readonly Cloudinary cloudinary;
    private readonly int maxIconSizeAllowed;

    public CloudinaryCloudService(ILogger<CloudinaryCloudService> logger, IConfiguration configuration)
    {
        this.logger = logger;

        cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
        cloudinary.Api.Secure = true;

        if (!int.TryParse(configuration["images:MaxIconSizeAllowed"], out maxIconSizeAllowed))
            maxIconSizeAllowed = 256 * 1024;
    }

    public async Task<AppResult<UploadResponse>> UploadSvgIcon(Stream icon)
    {
        var errorMessages = new List<string>();

        if (icon.Length > maxIconSizeAllowed || icon.Length <= 0)
            errorMessages.Add($"The icon size must not exceed {maxIconSizeAllowed / 1024} KB.");

        if (!IsSvgFile(icon))
            errorMessages.Add("The only icon format supported is SVG.");

        if (!errorMessages.IsNullOrEmpty())
            return AppResult<UploadResponse>.Failure(ErrorCode.UPLOADED_FILE_INVALID, errorMessages);

        var imageId = Guid.NewGuid();
        var uploadParams = new ImageUploadParams { File = new FileDescription(imageId.ToString(), icon), PublicId = imageId.ToString() };
        var uploadResult = await cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Upload failed for svg icon: {uploadResult.Error.Message}");

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
            stream.Position = 0; // Reset stream position
            var doc = XDocument.Load(stream); // Try to load the stream as XML
            stream.Position = 0; // Reset stream position again for further processing if needed

            // Ensure it contains <svg> root element
            return doc.Root?.Name.LocalName == "svg";
        }
        catch
        {
            return false; // If loading as XML fails, it's not a valid SVG
        }
    }
}
