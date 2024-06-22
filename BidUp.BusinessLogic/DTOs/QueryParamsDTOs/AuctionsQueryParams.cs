using BidUp.DataAccess.Entites;

namespace BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

public class AuctionsQueryParams : PaginationQueryParams
{
    private string? _search;
    public string? Search
    {
        get { return _search; }
        set { _search = value?.Trim().ToLower(); }
    }
    public bool ActiveOnly { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    public ProductCondition? ProductCondition { get; set; }
}
