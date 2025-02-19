using BidX.BusinessLogic.DTOs.CityDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface ICitiesService
{
    Task<IEnumerable<CityResponse>> GetCities();
}
