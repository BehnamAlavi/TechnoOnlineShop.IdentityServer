using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TechnoOnlineShop.IdentityServer.Application.Contracts.Options;

namespace TechnoOnlineShop.IdentityServer.Application
{
    public class JwtTokenService : IJwtTokenService
    {
        //private RSA _rsa;

        private readonly JwtOptions _jwtOptions;
        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<string> GenerateToken(
            string userId,
            string userName,
            IEnumerable<string> permissions)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.PreferredUsername, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            claims.AddRange(permissions.Select(p => new Claim("permission", p)));


            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(
                    await File.ReadAllTextAsync(_jwtOptions.PrivateKeyPath));

                var securityKey = new RsaSecurityKey(rsa)
                {
                    KeyId = _jwtOptions.Key
                };

                var credentials = new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.RsaSha256);

                var now = DateTime.UtcNow;

                var token = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.AddMinutes(15),
                    signingCredentials: credentials);

                // Signing happens here, while RSA is still alive.
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
       
    }

}
