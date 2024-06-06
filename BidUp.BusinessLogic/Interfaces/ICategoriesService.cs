using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryResponse>> GetCategories();
    Task<AppResult<CategoryResponse>> GetCategory(int id);
    Task<AppResult<CategoryResponse>> AddCategory(AddCategoryRequest addCategoryRequest, Stream categoryIcon);
    Task<AppResult> UpdateCategory(int id, UpdateCategoryRequest updateCategoryRequest, Stream? newCategoryIcon);
    Task<AppResult> DeleteCategory(int id);
}
