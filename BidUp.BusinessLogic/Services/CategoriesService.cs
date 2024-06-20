using AutoMapper;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CloudDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class CategoriesService : ICategoriesService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    private readonly ICloudService cloudService;

    public CategoriesService(AppDbContext appDbContext, IMapper mapper, ICloudService cloudService)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;
        this.cloudService = cloudService;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategories()
    {
        var categories = await appDbContext.Categories
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var response = mapper.Map<IEnumerable<CategoryResponse>>(categories);

        return response;
    }

    public async Task<AppResult<CategoryResponse>> GetCategory(int id)
    {
        var category = await appDbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category is null)
            return AppResult<CategoryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Category not found."]);

        var response = mapper.Map<CategoryResponse>(category);

        return AppResult<CategoryResponse>.Success(response);
    }

    public async Task<AppResult<CategoryResponse>> AddCategory(AddCategoryRequest addCategoryRequest, Stream categoryIcon)
    {
        var uploadResult = await cloudService.UploadSvgIcon(categoryIcon);

        if (!uploadResult.Succeeded)
            return AppResult<CategoryResponse>.Failure(uploadResult.Error!.ErrorCode, uploadResult.Error.ErrorMessages);

        var category = new Category
        {
            Name = addCategoryRequest.Name,
            IconUrl = uploadResult.Response!.FileUrl
        };

        appDbContext.Add(category);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<CategoryResponse>(category);

        return AppResult<CategoryResponse>.Success(response);
    }

    public async Task<AppResult> UpdateCategory(int id, UpdateCategoryRequest updateCategoryRequest, Stream? newCategoryIcon)
    {
        int noOfRowsAffected;

        if (newCategoryIcon is not null)
        {
            var uploadResult = await cloudService.UploadSvgIcon(newCategoryIcon);

            if (!uploadResult.Succeeded)
                return AppResult.Failure(uploadResult.Error!.ErrorCode, uploadResult.Error.ErrorMessages);

            noOfRowsAffected = await appDbContext.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Name, updateCategoryRequest.Name)
                    .SetProperty(c => c.IconUrl, uploadResult.Response!.FileUrl));
        }
        else
        {
            noOfRowsAffected = await appDbContext.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Name, updateCategoryRequest.Name));
        }

        if (noOfRowsAffected <= 0)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Category not found."]);

        return AppResult.Success();
    }

    public async Task<AppResult> DeleteCategory(int id)
    {
        var noOfRowsAffected = await appDbContext.Categories
            .Where(c => c.Id == id && !c.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.IsDeleted, true)); // Soft delete

        if (noOfRowsAffected <= 0)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Category not found."]);

        return AppResult.Success();
    }
}
