namespace BidUp.DataAccess.Entites;

public enum ProductCondition
{
    // Make its value starts with 1 because this will help me during the validation to ensure that the user enters a value and not leave it to its default value which is 0
    // Enums is a primitive non-nullable int so if the user didnt set it to a value it will use its default value which is 0 and wont return a "required" error for the user, See CreateAuctionRequest DTO to understand
    New = 1,
    Used
}
