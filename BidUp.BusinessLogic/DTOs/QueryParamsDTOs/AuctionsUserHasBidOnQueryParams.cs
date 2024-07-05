namespace BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

public class AuctionsUserHasBidOnQueryParams : PaginationQueryParams
{

    public bool ActiveOnly { get; set; }
    public bool WonOnly { get; set; }
}
