using BidX.BusinessLogic.DTOs.CategoryDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class CategoryMappingExtensions
{
    public static Category ToCategoryEntity(this AddCategoryRequest request, string iconUrl)
    {
        return new Category
        {
            Name = request.Name,
            IconUrl = iconUrl,
            IsDeleted = false
        };
    }

    public static CategoryResponse ToCategoryResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            IconUrl = category.IconUrl
        };
    }

    public static IQueryable<CategoryResponse> ProjectToCategoryResponse(this IQueryable<Category> query)
    {
        return query.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            IconUrl = c.IconUrl
        });
    }
}
