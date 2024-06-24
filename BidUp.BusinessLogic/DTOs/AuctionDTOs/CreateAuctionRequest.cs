using System.ComponentModel.DataAnnotations;
using BidUp.DataAccess.Entites;
using Newtonsoft.Json;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class CreateAuctionRequest
{
    [Required]
    public required string ProductName { get; init; }

    [Required] // To mark the filed as required in Swagger but does not have an effect for primitive non-nullable fields when using postman or other clients (see https://thom.ee/blog/clean-way-to-use-required-value-types-in-asp-net-core/)
    [Range(1, int.MaxValue, ErrorMessage = "The ProductCondition field is required.")] // I use [Range] to solve the problem of [Required] and make any primitive non-nullable field really required
    public required ProductCondition ProductCondition { get; init; }

    public string? ProductDescription { get; init; }

    [Required]
    [Range(1, ((double)decimal.MaxValue), ErrorMessage = "The StartingPrice field is required and must be a positive number.")]
    public decimal StartingPrice { get; init; }

    [Required]
    [Range(1, ((double)decimal.MaxValue), ErrorMessage = "The MinBidIncrement field is required and must be a positive number")]
    public decimal MinBidIncrement { get; init; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "The DurationInSeconds field is required and must be a positive number.")]
    public long DurationInSeconds { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The CategoryId field is required and must be a positive number.")]
    public int CategoryId { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The CityId field is required and must be a positive number.")]
    public int CityId { get; init; }
}