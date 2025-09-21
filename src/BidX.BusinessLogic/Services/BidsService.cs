using System.Text.Json;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Events;
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
        var contextData = await appDbContext.Auctions
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
        if (contextData is null)
        {
            await realTimeService.SendErrorToUser(bidderId, ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);
            return;
        }

        var validationResult = ValidateBidPlacement(
            auctionEndTime: contextData.EndTime,
            auctionStartingPrice: contextData.StartingPrice,
            highestBidAmount: contextData.HighestBid?.Amount,
            auctionMinBidIncrement: contextData.MinBidIncrement,
            auctioneerId: contextData.AuctioneerId,
            bidderId: bidderId,
            bidAmount: request.Amount);

        if (!validationResult.Succeeded)
        {
            await realTimeService.SendErrorToUser(bidderId, validationResult.Error!);
            return;
        }

        // Create and save the bid
        var bid = request.ToBidEntity(bidderId);
        appDbContext.Add(bid);

        // Create the outbox message
        var bidPlacedEvent = new BidPlacedEvent
        {
            BidAmount = bid.Amount,
            BidderId = bidderId,
            AuctionId = request.AuctionId,
            AuctionProductName = contextData.ProductName,
            AuctioneerId = contextData.AuctioneerId,
            PreviousHighBidderId = contextData.HighestBid?.BidderId
        };

        var outboxMessage = new OutboxMessage
        {
            Type = typeof(BidPlacedEvent).FullName!,
            Content = JsonSerializer.Serialize(bidPlacedEvent),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        appDbContext.Add(outboxMessage);

        await appDbContext.SaveChangesAsync();

        var bidResponse = bid.ToBidResponse(contextData.Bidder!.FullName, contextData.Bidder.ProfilePictureUrl, contextData.Bidder.AverageRating);
        await Task.WhenAll(
            realTimeService.SendPlacedBidToAuctionRoom(bidResponse.AuctionId, bidResponse),
            realTimeService.UpdateAuctionPriceInFeed(bidResponse.AuctionId, bidResponse.Amount)
        );
    }

    public async Task AcceptBid(int callerId, AcceptBidRequest request)
    {
        var contextData = await appDbContext.Bids
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
        var validationResult = ValidateBidAcceptance(callerId, contextData?.Bid);
        if (!validationResult.Succeeded)
        {
            await realTimeService.SendErrorToUser(callerId, validationResult.Error!);
            return;
        }

        AcceptBidAndEndAuction(contextData!.Bid);

        // Create the outbox message for bid accepted event
        var bidAcceptedEvent = new BidAcceptedEvent
        {
            WinnerId = contextData.Bidder.Id,
            AuctionId = contextData.Bid.Auction!.Id,
            AuctionProductName = contextData.Bid.Auction.ProductName,
            AuctioneerId = contextData.Bid.Auction.AuctioneerId,
            BiddersIds = contextData.BidderIds.ToList()
        };

        var outboxMessage = new OutboxMessage
        {
            Type = typeof(BidAcceptedEvent).FullName!,
            Content = JsonSerializer.Serialize(bidAcceptedEvent),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        appDbContext.Add(outboxMessage);

        await appDbContext.SaveChangesAsync();

        // Send the realtime updates
        var response = contextData.Bid.ToBidResponse(contextData.Bidder!.FullName, contextData.Bidder.ProfilePictureUrl, contextData.Bidder.AverageRating);
        await Task.WhenAll(
            realTimeService.SendAcceptedBidToAuctionRoom(response.AuctionId, response),
            realTimeService.MarkAuctionAsEndedInFeed(response.AuctionId, response.Amount));
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