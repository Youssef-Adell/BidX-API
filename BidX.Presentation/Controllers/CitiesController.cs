using BidX.BusinessLogic.DTOs.CityDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidX.Presentation.Controllers;

[ApiController]
[Route("api/cities")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class CitiesController : ControllerBase
{
    private readonly ICitiesService citiesService;

    public CitiesController(ICitiesService citiesService)
    {
        this.citiesService = citiesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities()
    {
        var response = await citiesService.GetCities();

        return Ok(response);
    }
}
