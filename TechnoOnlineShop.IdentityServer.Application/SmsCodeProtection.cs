using System.Security.Cryptography;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Application
{
    public static class SmsCodeProtection
    {
        public static string ComputeMac(string challengeId, string code, byte[] secretKey)
        {
            var input = $"{challengeId}:{code}";
            var hash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
    }
}
