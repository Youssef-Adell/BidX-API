using System.ComponentModel.DataAnnotations;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
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
}
