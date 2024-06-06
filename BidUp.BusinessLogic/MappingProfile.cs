using AutoMapper;
using BidUp.BusinessLogic.DTOs.CategoryDTOs;
using BidUp.BusinessLogic.DTOs.CityDTOs;
using BidUp.DataAccess.Entites;

namespace BidUp.BusinessLogic;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<City, CityResponse>();
        CreateMap<Category, CategoryResponse>();
    }
}