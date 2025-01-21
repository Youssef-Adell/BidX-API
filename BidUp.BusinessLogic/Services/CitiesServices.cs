using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

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
