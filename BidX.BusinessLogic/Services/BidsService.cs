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
                HighestBid = a.Bids! // Needed for validation and notification sending
                    .OrderByDescending(b => b.Amount)
                    .Select(b => new { b.BidderId, b.Amount })
                    .FirstOrDefault(),
                Bidder = appDbContext.Users // Needed to construct the BidResponse
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


        using var transaction = await appDbContext.Database.BeginTransactionAsync();
        try
        {
            // Create and save the bid
            var bid = request.ToBidEntity(bidderId);
            appDbContext.Bids.Add(bid);
            await appDbContext.SaveChangesAsync();

            // Send the notifications
            await notificationsService.SendPlacedBidNotifications(
                auctionId: request.AuctionId,
                auctionTitle: auctionInfo.ProductName,
                bidAmount: bid.Amount,
                bidderId: bidderId,
                auctioneerId: auctionInfo.AuctioneerId,
                previousHighestBidderId: auctionInfo.HighestBid?.BidderId);

            // Send the realtime updates
            var response = bid.ToBidResponse(auctionInfo.Bidder.FullName, auctionInfo.Bidder.ProfilePictureUrl, auctionInfo.Bidder.AverageRating);
            await Task.WhenAll(
                realTimeService.SendPlacedBidToAuctionRoom(response.AuctionId, response),
                realTimeService.UpdateAuctionPriceInFeed(response.AuctionId, response.Amount));

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            await realTimeService.SendErrorToUser(bidderId, ErrorCode.SERVER_INTENRAL_ERROR, ["An error occurred while placing the bid."]);
        }
    }

    public async Task AcceptBid(int callerId, AcceptBidRequest request)
    {
        var bidInfo = await appDbContext.Bids
            .Where(b => b.Id == request.BidId)
            .Include(b => b.Auction)
            .Select(b => new
            {
                Bid = b,
                Bidder = new // Needed for mapping to BidResponse
                {
                    b.Bidder!.FullName,
                    b.Bidder.ProfilePictureUrl,
                    b.Bidder.AverageRating,
                    b.Bidder.Id
                },
                BidderIds = b.Auction!.Bids! // Needed for notifications sending
                    .Select(b => b.BidderId)
                    .Distinct()
            })
            .SingleOrDefaultAsync();


        // Validate
        var validationResult = ValidateBidAcceptance(callerId, bidInfo?.Bid);
        if (!validationResult.Succeeded)
        {
            await realTimeService.SendErrorToUser(callerId, validationResult.Error!);
            return;
        }

        using var transaction = await appDbContext.Database.BeginTransactionAsync();
        try
        {
            // Accept and save
            AcceptBidAndEndAuction(bidInfo!.Bid);
            await appDbContext.SaveChangesAsync();

            // Send the notifications
            await notificationsService.SendAcceptedBidNotifications(
                auctionId: bidInfo.Bid.Auction!.Id,
                auctionTitle: bidInfo.Bid.Auction.ProductName,
                winnerId: bidInfo.Bidder.Id,
                auctioneerId: bidInfo.Bid.Auction.AuctioneerId,
                biddersIds: bidInfo.BidderIds);

            // Send the realtime updates
            var response = bidInfo.Bid.ToBidResponse(bidInfo.Bidder!.FullName, bidInfo.Bidder.ProfilePictureUrl, bidInfo.Bidder.AverageRating);
            await Task.WhenAll(
                realTimeService.SendAcceptedBidToAuctionRoom(response.AuctionId, response),
                realTimeService.MarkAuctionAsEndedInFeed(response.AuctionId, response.Amount));

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            await realTimeService.SendErrorToUser(callerId, ErrorCode.SERVER_INTENRAL_ERROR, ["An error occurred while accepting the bid."]);
        }
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