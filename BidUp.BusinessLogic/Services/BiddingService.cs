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

    public async Task<AppResult<IEnumerable<BidResponse>>> GetAuctionBids(int auctionId)
    {
        var auctionExists = await appDbContext.Auctions
            .AnyAsync(a => a.Id == auctionId);

        if (!auctionExists)
            return AppResult<IEnumerable<BidResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        var bids = await appDbContext.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .AsNoTracking()
            .ToListAsync();

        var response = mapper.Map<IEnumerable<Bid>, IEnumerable<BidResponse>>(bids);

        return AppResult<IEnumerable<BidResponse>>.Success(response);
    }

    public async Task<AppResult<BidResponse>> GetAcceptedBid(int auctionId)
    {
        var auction = await appDbContext.Auctions
            .Include(a => a.HighestBid)
                .ThenInclude(b => b!.Bidder)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        if (auction.HighestBid is null || auction.WinnerId is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no accepted bid."]);

        var response = mapper.Map<Bid, BidResponse>(auction.HighestBid);

        return AppResult<BidResponse>.Success(response);
    }

    public async Task<AppResult<BidResponse>> GetHighestBid(int auctionId)
    {
        var auction = await appDbContext.Auctions
            .Include(a => a.HighestBid)
                .ThenInclude(b => b!.Bidder)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        if (auction.HighestBid is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no bids"]);

        var response = mapper.Map<Bid, BidResponse>(auction.HighestBid);

        return AppResult<BidResponse>.Success(response);
    }

    public async Task<AppResult<BidResponse>> BidUp(int bidderId, BidRequest bidRequest)
    {
        var auction = await appDbContext.Auctions
            .Include(a => a.HighestBid)
            .FirstOrDefaultAsync(a => a.Id == bidRequest.AuctionId);

        if (auction is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        if (!auction.IsActive)
            return AppResult<BidResponse>.Failure(ErrorCode.AUCTION_ENDED, ["Auction has ended."]);

        var bidValidationResult = ValidateBid(bidderId, auction!, bidRequest);
        if (!bidValidationResult.Succeeded)
            return AppResult<BidResponse>.Failure(bidValidationResult.Error!.ErrorCode, bidValidationResult.Error.ErrorMessages);

        // Create the bid and assign it to the auction as the highest bid
        var bid = mapper.Map<BidRequest, Bid>(bidRequest);
        bid.BidderId = bidderId;
        auction!.HighestBid = bid;
        await appDbContext.SaveChangesAsync();

        // Map the bid to a BidResponse
        bid.Bidder = appDbContext.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == bidderId);
        var response = mapper.Map<Bid, BidResponse>(bid);

        return AppResult<BidResponse>.Success(response);
    }

    public async Task<AppResult<BidResponse>> AcceptBid(int currentUserId, AcceptBidRequest acceptBidRequest)
    {
        var bid = await appDbContext.Bids
            .Include(b => b.Auction)
            .Include(b => b.Bidder) // Needed for BidResponse that will be returned
            .FirstOrDefaultAsync(b => b.Id == acceptBidRequest.BidId);

        if (bid is null)
            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Bid not found."]);

        var auction = bid.Auction!;

        if (auction.AuctioneerId != currentUserId)
            return AppResult<BidResponse>.Failure(ErrorCode.PERMISSION_DENIED, ["Only the auction owner can accept this bid."]);

        if (!auction.IsActive)
            return AppResult<BidResponse>.Failure(ErrorCode.AUCTION_ENDED, ["Auction has ended. Bid acceptance is no longer available."]);

        // Set the winner and end the auction
        auction.WinnerId = bid.BidderId;
        auction.HighestBidId = bid.Id;
        auction.EndTime = DateTime.UtcNow;
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Bid, BidResponse>(bid);
        return AppResult<BidResponse>.Success(response);
    }

    private AppResult ValidateBid(int bidderId, Auction auction, BidRequest bidRequest)
    {
        if (auction.AuctioneerId == bidderId)
            return AppResult.Failure(ErrorCode.BID_NOT_ALLOWED, ["You can't bid on your own auction."]);

        if (bidRequest.Amount <= auction.HighestBid?.Amount)
            return AppResult.Failure(ErrorCode.BID_AMOUNT_INVALID, ["Your bid must be greater than the last bid."]);

        var currentBidInrement = bidRequest.Amount - auction.CurrentPrice;
        if (currentBidInrement < auction.MinBidIncrement)
            return AppResult.Failure(ErrorCode.BID_AMOUNT_INVALID, [$"Your bid increment must be greater than or equal to {auction.MinBidIncrement}."]);

        return AppResult.Success();
    }
}
