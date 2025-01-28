using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Extensions;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class BidsService : IBidsService
{
    private readonly AppDbContext appDbContext;
    private readonly IRealTimeService realTimeService;
    private readonly INotificationsService notificationsService;

    public BidsService(AppDbContext appDbContext, IRealTimeService realTimeService, INotificationsService notificationsService)
    {
        this.appDbContext = appDbContext;
        this.realTimeService = realTimeService;
        this.notificationsService = notificationsService;
    }

    public async Task<Result<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams)
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
                return Result<Page<BidResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return Result<Page<BidResponse>>.Success(new([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of bids with pagination and mapping
        var auctionBids = await auctionBidsQuery
            .OrderByDescending(a => a.Id)
            .ProjectToBidResponse()
            .Paginate(queryParams.Page, queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var response = new Page<BidResponse>(auctionBids, queryParams.Page, queryParams.PageSize, totalCount);
        return Result<Page<BidResponse>>.Success(response);
    }


    public async Task<Result<BidResponse>> GetAcceptedBid(int auctionId)
    {
        var acceptedBid = await appDbContext.Bids
            .Include(b => b.Bidder)
            .ProjectToBidResponse()
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.AuctionId == auctionId && b.IsAccepted);

        if (acceptedBid is null)
        {
            var auctionExists = await appDbContext.Auctions.AnyAsync(a => a.Id == auctionId);
            if (!auctionExists)
                return Result<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return Result<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no accepted bid."]);
        }

        return Result<BidResponse>.Success(acceptedBid);
    }

    public async Task<Result<BidResponse>> GetHighestBid(int auctionId)
    {
        var highestBid = await appDbContext.Bids
            .Include(b => b.Bidder)
            .OrderByDescending(b => b.Amount)
            .ProjectToBidResponse()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.AuctionId == auctionId);

        if (highestBid is null)
        {
            var auctionExists = await appDbContext.Auctions.AnyAsync(a => a.Id == auctionId);
            if (!auctionExists)
                return Result<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

            return Result<BidResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction has no bids"]);
        }

        return Result<BidResponse>.Success(highestBid);
    }

    public async Task PlaceBid(int bidderId, BidRequest request)
    {
        var auctionInfo = await appDbContext.Auctions
            .AsNoTracking()
            .Where(a => a.Id == request.AuctionId)
            .Select(a => new
            {
                a.ProductName,
                a.AuctioneerId,
                a.MinBidIncrement,
                a.EndTime,
                a.StartingPrice,
                HighestBid = a.Bids!
                    .OrderByDescending(b => b.Amount)
                    .Select(b => new { b.BidderId, b.Amount })
                    .FirstOrDefault(),
                CurrentBidder = appDbContext.Users
                    .Where(u => u.Id == bidderId)
                    .Select(u => new { u!.FullName, u.ProfilePictureUrl, u.AverageRating })
                    .Single()
            })
            .SingleOrDefaultAsync();


        // Validate
        if (auctionInfo is null)
        {
            await realTimeService.SendErrorToUser(bidderId, ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);
            return;
        }

        var validationResult = ValidateBidPlacement(
            auctionEndTime: auctionInfo.EndTime,
            auctionStartingPrice: auctionInfo.StartingPrice,
            highestBidAmount: auctionInfo.HighestBid?.Amount,
            auctionMinBidIncrement: auctionInfo.MinBidIncrement,
            auctioneerId: auctionInfo.AuctioneerId,
            bidderId: bidderId,
            bidAmount: request.Amount);

        if (!validationResult.Succeeded)
        {
            await realTimeService.SendErrorToUser(bidderId, validationResult.Error!);
            return;
        }


        // Create and save the bid
        var bid = request.ToBidEntity(bidderId);
        appDbContext.Bids.Add(bid);
        await appDbContext.SaveChangesAsync();


        // Send the realtime updates and notifications
        var response = bid.ToBidResponse(
            auctionInfo.CurrentBidder.FullName,
            auctionInfo.CurrentBidder.ProfilePictureUrl,
            auctionInfo.CurrentBidder.AverageRating);

        await Task.WhenAll(
            realTimeService.SendPlacedBidToAuctionRoom(response.AuctionId, response),
            realTimeService.UpdateAuctionPriceInFeed(response.AuctionId, response.Amount),
            notificationsService.NotifyNewBid(
                auctionId: request.AuctionId,
                auctionTitle: auctionInfo.ProductName,
                bidAmount: response.Amount,
                bidderId: bidderId,
                auctioneerId: auctionInfo.AuctioneerId,
                previousHighestBidderId: auctionInfo.HighestBid?.BidderId)
        );
    }

    public async Task AcceptBid(int callerId, AcceptBidRequest request)
    {
        var bid = await appDbContext.Bids
            .Include(b => b.Auction)
            .Include(b => b.Bidder) // Needed for BidResponse that will be returned
            .SingleOrDefaultAsync(b => b.Id == request.BidId);

        var validationResult = ValidateBidAcceptance(callerId, bid);
        if (!validationResult.Succeeded)
        {
            await realTimeService.SendErrorToUser(callerId, validationResult.Error!);
            return;
        }

        AcceptBidAndEndAuction(bid!);
        await appDbContext.SaveChangesAsync();

        // Send the updates via the realtime connection
        var response = bid!.ToBidResponse();
        await Task.WhenAll(
            realTimeService.SendAcceptedBidToAuctionRoom(response.AuctionId, response),
            realTimeService.MarkAuctionAsEndedInFeed(response.AuctionId, response.Amount)
        );
    }


    private Result ValidateBidPlacement(DateTimeOffset auctionEndTime, decimal auctionStartingPrice, decimal? highestBidAmount, decimal auctionMinBidIncrement, int auctioneerId, int bidderId, decimal bidAmount)
    {
        var auctionCurrentPrice = highestBidAmount ?? auctionStartingPrice;

        if (auctioneerId == bidderId)
            return Result.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Auctioneer cannot bid on their own auction."]);

        if (auctionEndTime.CompareTo(DateTimeOffset.UtcNow) < 0)
            return Result.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Auction has ended."]);

        if (bidAmount <= auctionCurrentPrice)
            return Result.Failure(ErrorCode.BIDDING_NOT_ALLOWED, ["Bid amount must be greater than the current price."]);

        if (bidAmount - auctionCurrentPrice < auctionMinBidIncrement)
            return Result.Failure(ErrorCode.BIDDING_NOT_ALLOWED, [$"Bid increment must be greater than or equal to {auctionMinBidIncrement}."]);

        return Result.Success();
    }

    private Result ValidateBidAcceptance(int callerId, Bid? bid)
    {
        if (bid is null)
            return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Bid not found."]);

        if (bid.Auction!.AuctioneerId != callerId)
            return Result.Failure(ErrorCode.ACCEPTANCE_NOT_ALLOWED, ["Only the auction owner can accept this bid."]);

        if (!bid.Auction.IsActive)
            return Result.Failure(ErrorCode.ACCEPTANCE_NOT_ALLOWED, ["Auction has ended. Acceptance is no longer allowed."]);

        return Result.Success();
    }

    private void AcceptBidAndEndAuction(Bid bid)
    {
        bid.IsAccepted = true;

        var auction = bid.Auction!;
        auction.WinnerId = bid.BidderId;
        auction.EndTime = DateTimeOffset.UtcNow;
    }
}