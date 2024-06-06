using BidUp.BusinessLogic.DTOs.CityDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface ICitiesService
{
    Task<IEnumerable<CityResponse>> GetCities();
}
