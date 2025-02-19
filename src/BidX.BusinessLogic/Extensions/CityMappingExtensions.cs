using BidX.BusinessLogic.DTOs.CityDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class CityMappingExtensions
{
    public static IQueryable<CityResponse> ProjectToCityResponse(this IQueryable<City> query)
    {
        return query.Select(c => new CityResponse
        {
            Id = c.Id,
            Name = c.Name
        });
    }
}
