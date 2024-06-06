using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

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
