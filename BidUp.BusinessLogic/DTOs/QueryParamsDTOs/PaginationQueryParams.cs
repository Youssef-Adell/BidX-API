namespace BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

public class PaginationQueryParams
{
    private const int MaxPageSize = 30;
    private int _page = 1;
    private int _pageSize = 5;

    public int Page
    {
        get => _page;
        set => _page = (value > 0) ? value : _page;
    }
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (MaxPageSize >= value && value > 0) ? value : MaxPageSize;
    }
}
