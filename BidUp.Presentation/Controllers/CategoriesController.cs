using System.ComponentModel.DataAnnotations;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        this.categoriesService = categoriesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var response = await categoriesService.GetCategories();

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(int id)
    {
        var result = await categoriesService.GetCategory(id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    /// <summary>
    /// [Can be called by admins only]
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// [Can be called by admins only]
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// [Can be called by admins only]
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await categoriesService.DeleteCategory(id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return NoContent();
    }

}
