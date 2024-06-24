using AutoMapper;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class BiddingService : IBiddingService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;


    public BiddingService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;
    }


    public async Task<AppResult<BidResponse>> BidUp(int bidderId, BidRequest bidRequest)
    {
        // ensure that the auction is exist & active
        var auction = await appDbContext.Auctions
            .Include(a => a.HighestBid)
            .FirstOrDefaultAsync(a => a.Id == bidRequest.AuctionId);

        if (auction is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        if (!auction.IsActive)
            return AppResult<BidResponse>.Failure(ErrorCode.AUCTION_ENDED, ["Auction has ended."]);

        // ensure that this bid amount > the last bid amount and current bid Increment > min bid increment
        if (bidRequest.Amount <= auction.HighestBid?.Amount)
            return AppResult<BidResponse>.Failure(ErrorCode.BID_AMOUNT_INVALID, ["Your bid must be greater than the last bid."]);

        var currentBidInrement = bidRequest.Amount - (auction.HighestBid is not null ? auction.HighestBid.Amount : auction.StartingPrice);

        if (currentBidInrement < auction.MinBidIncrement)
            return AppResult<BidResponse>.Failure(ErrorCode.BID_AMOUNT_INVALID, [$"Your bid increment be greater than {auction.MinBidIncrement}."]);

        // create the bid and assign it to the auction as the highest bid
        var bid = mapper.Map<BidRequest, Bid>(bidRequest);
        bid.BidderId = bidderId;

        auction.HighestBid = bid;
        await appDbContext.SaveChangesAsync();

        // map the bid to bidResponse and return it
        bid.Bidder = appDbContext.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == bidderId);

        var response = mapper.Map<Bid, BidResponse>(bid);
        return AppResult<BidResponse>.Success(response);
    }

}
