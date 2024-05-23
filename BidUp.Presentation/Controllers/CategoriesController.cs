using BidUp.BusinessLogic.Interfaces;
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
    public async Task<IActionResult> GetCities()
    {
        var response = await categoriesService.GetCategories();

        return Ok(response);
    }
}
