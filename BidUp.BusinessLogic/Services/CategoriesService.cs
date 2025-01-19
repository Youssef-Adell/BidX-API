using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
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
            .ProjectTo<CategoryResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        return categories;
    }

    public async Task<AppResult<CategoryResponse>> GetCategory(int id)
    {
        var category = await appDbContext.Categories
            .Where(c => c.Id == id && !c.IsDeleted)
            .ProjectTo<CategoryResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .SingleOrDefaultAsync();

        if (category is null)
            return AppResult<CategoryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Category not found."]);

        return AppResult<CategoryResponse>.Success(category);
    }

    public async Task<AppResult<CategoryResponse>> AddCategory(AddCategoryRequest request, Stream categoryIcon)
    {
        var uploadResult = await cloudService.UploadSvgIcon(categoryIcon);
        if (!uploadResult.Succeeded)
            return AppResult<CategoryResponse>.Failure(uploadResult.Error!);

        var category = mapper.Map<AddCategoryRequest, Category>(request, o => o.Items["IconUrl"] = uploadResult.Response!.FileUrl);
        appDbContext.Add(category);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Category, CategoryResponse>(category);
        return AppResult<CategoryResponse>.Success(response);
    }

    public async Task<AppResult> UpdateCategory(int id, UpdateCategoryRequest request, Stream? newCategoryIcon)
    {
        string? iconUrl = null;
        if (newCategoryIcon is not null)
        {
            var uploadResult = await cloudService.UploadSvgIcon(newCategoryIcon);
            if (!uploadResult.Succeeded)
                return AppResult.Failure(uploadResult.Error!);

            iconUrl = uploadResult.Response!.FileUrl;
        }

        var noOfRowsAffected = await appDbContext.Categories
            .Where(c => c.Id == id && !c.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Name, request.Name)
                .SetProperty(c => c.IconUrl, c => iconUrl ?? c.IconUrl));

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
