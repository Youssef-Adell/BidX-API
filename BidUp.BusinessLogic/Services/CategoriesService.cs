using AutoMapper;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class CategoriesService : ICategoriesService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public CategoriesService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;

    }

    public async Task<IEnumerable<CategoryResponse>> GetCategories()
    {
        var categories = await appDbContext.Categories.AsNoTracking().ToListAsync();

        var response = mapper.Map<IEnumerable<CategoryResponse>>(categories);

        return response;
    }

    public async Task<AppResult<CategoryResponse>> GetCategory(int id)
    {
        var category = await appDbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return AppResult<CategoryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Category not found."]);

        var response = mapper.Map<CategoryResponse>(category);

        return AppResult<CategoryResponse>.Success(response);
    }
}
