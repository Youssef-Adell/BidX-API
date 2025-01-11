using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class BidsService : IBidsService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public BidsService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;
    }

    public async Task<AppResult<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams)
    {
        // Build the query based on the parameters
        var auctionBidsQuery = appDbContext.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId);

        // Get the total count before pagination
        var totalCount = await auctionBidsQuery.CountAsync();

        if (totalCount == 0)
        {
            var auctionExists = await appDbContext.Auctions.AnyAsync(a => a.Id == auctionId);
            if (!auctionExists)
                return AppResult<Page<BidResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return AppResult<Page<BidResponse>>.Success(new([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of bids with pagination and mapping
        var auctionBids = await auctionBidsQuery
            // Get the last bids first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ProjectTo<BidResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        var response = new Page<BidResponse>(auctionBids, queryParams.Page, queryParams.PageSize, totalCount);
        return AppResult<Page<BidResponse>>.Success(response);
    }


    public async Task<AppResult<BidResponse>> GetAcceptedBid(int auctionId)
    {
        var acceptedBid = await appDbContext.Bids
            .Include(b => b.Bidder)
            .ProjectTo<BidResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.AuctionId == auctionId && b.IsAccepted);

        if (acceptedBid is null)
        {
            var auctionExists = await appDbContext.Auctions.AnyAsync(a => a.Id == auctionId);
            if (!auctionExists)
                return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no accepted bid."]);
        }

        return AppResult<BidResponse>.Success(acceptedBid);
    }

    public async Task<AppResult<BidResponse>> GetHighestBid(int auctionId)
    {
        var highestBid = await appDbContext.Bids
            .Include(b => b.Bidder)
            .OrderByDescending(b => b.Amount)
            .ProjectTo<BidResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.AuctionId == auctionId);

        if (highestBid is null)
        {
            var auctionExists = await appDbContext.Auctions.AnyAsync(a => a.Id == auctionId);
            if (!auctionExists)
                return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return AppResult<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no bids"]);
        }

        return AppResult<BidResponse>.Success(highestBid);
    }

    public async Task<AppResult<BidResponse>> PlaceBid(int bidderId, BidRequest request)
    {
        var validationResult = await ValidateBidPlacement(bidderId, request.AuctionId, request.Amount);
        if (!validationResult.Succeeded)
            return AppResult<BidResponse>.Failure(validationResult.Error!);

        // Create and save the bid
        var bid = mapper.Map<BidRequest, Bid>(request, o => o.Items["BidderId"] = bidderId);
        appDbContext.Bids.Add(bid);
        await appDbContext.SaveChangesAsync();

        await appDbContext.Entry(bid).Reference(b => b.Bidder).LoadAsync();
        var response = mapper.Map<Bid, BidResponse>(bid);

        return AppResult<BidResponse>.Success(response);
    }

    public async Task<AppResult<BidResponse>> AcceptBid(int callerId, AcceptBidRequest request)
    {
        var bid = await appDbContext.Bids
            .Include(b => b.Auction)
            .Include(b => b.Bidder) // Needed for BidResponse that will be returned
            .SingleOrDefaultAsync(b => b.Id == request.BidId);

        var validationResult = ValidateBidAcceptance(callerId, bid);
        if (!validationResult.Succeeded)
            return AppResult<BidResponse>.Failure(validationResult.Error!);

        AcceptBidAndEndAuction(bid!);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Bid, BidResponse>(bid!);
        return AppResult<BidResponse>.Success(response);
    }


    private async Task<AppResult> ValidateBidPlacement(int bidderId, int auctionId, decimal bidAmount)
    {
        // Load only the necessary auction information
        var auctionInfo = await appDbContext.Auctions
            .AsNoTracking()
            .Select(a => new
            {
                a.Id,
                a.AuctioneerId,
                a.MinBidIncrement,
                a.EndTime,
                CurrentPrice = a.Bids!.OrderByDescending(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault() ?? a.StartingPrice
            })
            .SingleOrDefaultAsync(x => x.Id == auctionId);

        if (auctionInfo is null)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        if (auctionInfo.AuctioneerId == bidderId)
            return AppResult.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Auctioneer cannot bid on their own auction."]);

        if (auctionInfo.EndTime.CompareTo(DateTimeOffset.UtcNow) < 0)
            return AppResult.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Auction has ended."]);

        if (bidAmount <= auctionInfo.CurrentPrice)
            return AppResult.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Bid amount must be greater than the current price."]);

        if (bidAmount - auctionInfo.CurrentPrice < auctionInfo.MinBidIncrement)
            return AppResult.Failure(ErrorCode.BIDDING_NOT_ALLOWED, [$"Bid increment must be greater than or equal to {auctionInfo.MinBidIncrement}."]);

        return AppResult.Success();
    }

    private AppResult ValidateBidAcceptance(int callerId, Bid? bid)
    {
        if (bid is null)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Bid not found."]);

        if (bid.Auction!.AuctioneerId != callerId)
            return AppResult.Failure(ErrorCode.ACCEPTANCE_NOT_ALLOWED, ["Only the auction owner can accept this bid."]);

        if (!bid.Auction.IsActive)
            return AppResult.Failure(ErrorCode.ACCEPTANCE_NOT_ALLOWED, ["Auction has ended. Acceptance is no longer allowed."]);

        return AppResult.Success();
    }

    private void AcceptBidAndEndAuction(Bid bid)
    {
        bid.IsAccepted = true;

        var auction = bid.Auction!;
        auction.WinnerId = bid.BidderId;
        auction.EndTime = DateTimeOffset.UtcNow;
    }
}