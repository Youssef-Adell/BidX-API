namespace BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

public class UserAuctionsQueryParams : PaginationQueryParams
{
    public bool ActiveOnly { get; set; }
}
