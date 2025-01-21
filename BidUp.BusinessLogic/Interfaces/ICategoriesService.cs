using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryResponse>> GetCategories();
    Task<Result<CategoryResponse>> GetCategory(int id);
    Task<Result<CategoryResponse>> AddCategory(AddCategoryRequest request, Stream categoryIcon);
    Task<Result> UpdateCategory(int id, UpdateCategoryRequest request, Stream? newCategoryIcon);
    Task<Result> DeleteCategory(int id);
}
