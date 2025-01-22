using System;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Retrieves the user ID from the ClaimsPrincipal as an integer.
    /// Throws an exception if the user ID is missing or invalid.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "ClaimsPrincipal is null.");
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim is missing.");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("User ID claim is not a valid integer.");
        }

        return userId;
    }
}
