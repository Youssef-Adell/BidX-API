using BidX.BusinessLogic.DTOs.CityDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class CitiesServices : ICitiesService
{
    private readonly AppDbContext appDbContext;

    public CitiesServices(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task<IEnumerable<CityResponse>> GetCities()
    {
        var cities = await appDbContext.Cities
            .ProjectToCityResponse()
            .AsNoTracking()
            .ToListAsync();

        return cities;
    }

}
