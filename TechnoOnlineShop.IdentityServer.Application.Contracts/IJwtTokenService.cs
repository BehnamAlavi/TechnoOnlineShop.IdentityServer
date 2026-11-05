namespace TechnoOnlineShop.IdentityServer.Application
{
    public interface IJwtTokenService
    {
       Task<string> GenerateToken(string userId, string userName, IEnumerable<string> permissions);
    }
}