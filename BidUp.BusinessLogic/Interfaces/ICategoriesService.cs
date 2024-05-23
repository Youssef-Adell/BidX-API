using BidUp.BusinessLogic.DTOs.CategoryDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryResponse>> GetCategories();
}
