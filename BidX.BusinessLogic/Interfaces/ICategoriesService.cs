using BidX.BusinessLogic.DTOs.CategoryDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryResponse>> GetCategories();
    Task<Result<CategoryResponse>> GetCategory(int id);
    Task<Result<CategoryResponse>> AddCategory(AddCategoryRequest request, Stream categoryIcon);
    Task<Result> UpdateCategory(int id, UpdateCategoryRequest request, Stream? newCategoryIcon);
    Task<Result> DeleteCategory(int id);
}
