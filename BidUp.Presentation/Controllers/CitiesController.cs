using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICitiesService citiesService;

    public CitiesController(ICitiesService citiesService)
    {
        this.citiesService = citiesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
        var response = await citiesService.GetCities();

        return Ok(response);
    }
}
