using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidX.BusinessLogic.DTOs.CityDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class CitiesServices : ICitiesService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public CitiesServices(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;

    }

    public async Task<IEnumerable<CityResponse>> GetCities()
    {
        var cities = await appDbContext.Cities
            .ProjectTo<CityResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        return cities;
    }

}
