using System.ComponentModel.DataAnnotations;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        this.categoriesService = categoriesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var response = await categoriesService.GetCategories();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var result = await categoriesService.GetCategory(id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddCategory([FromForm] AddCategoryRequest addCategoryRequest, [Required] IFormFile icon)
    {
        using (var categoryIconStream = icon.OpenReadStream())
        {
            var result = await categoriesService.AddCategory(addCategoryRequest, categoryIconStream);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            return CreatedAtAction(nameof(GetCategory), new { id = result.Response!.Id }, result.Response);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromForm] UpdateCategoryRequest updateCategoryRequest, IFormFile? icon)
    {
        // If categoryIconStream is null, the using statement effectively does nothing with regard to resource disposal since there is no resource to dispose of.
        // The body of the using statement will still execute with categoryIconStream being null.

        using (var categoryIconStream = icon?.OpenReadStream())
        {
            var result = await categoriesService.UpdateCategory(id, updateCategoryRequest, categoryIconStream);

            if (!result.Succeeded)
            {
                var errorCode = result.Error!.ErrorCode;
                if (errorCode == ErrorCode.RESOURCE_NOT_FOUND)
                    return NotFound(result.Error);

                else if (errorCode == ErrorCode.UPLOADED_FILE_INVALID)
                    return UnprocessableEntity(result.Error);
            }
        }

        return NoContent();
    }
}
