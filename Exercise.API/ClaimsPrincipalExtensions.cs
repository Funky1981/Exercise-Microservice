using System.Security.Claims;

namespace Exercise.API
{
    /// <summary>
    /// Extension helpers for ClaimsPrincipal used across all minimal API endpoints.
    /// Centralises the repeated pattern of extracting the user ID from the JWT sub claim.
    /// </summary>
    internal static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Tries to parse the authenticated user's ID from the <c>sub</c> or
        /// <c>NameIdentifier</c> claim.
        /// </summary>
        /// <returns>
        /// <c>true</c> and a valid <see cref="Guid"/> when the claim exists and parses;
        /// <c>false</c> otherwise (caller should return 401 Unauthorized).
        /// </returns>
        public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
        {
            var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst("sub")?.Value;

            return Guid.TryParse(value, out userId);
        }
    }
}
