using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RevisiaAPI.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RevisiaAPI.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration config)
        {
            _secret = config["JWT_SECRET"] ?? throw new Exception("JWT_SECRET missing");
            _issuer = config["JWT_ISSUER"] ?? throw new Exception("JWT_ISSUER missing");
            _audience = config["JWT_AUDIENCE"] ?? throw new Exception("JWT_AUDIENCE missing");
        }

        public string GenerateToken(User user, int validMinutes = 1440)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(validMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(int daysValid)
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            string token = Convert.ToBase64String(randomBytes);
            string hashedValue = HashToken(token);
            var expiry = DateTime.UtcNow.AddDays(daysValid);
            return new RefreshToken
            {
                Token = token,
                HashedValue = hashedValue,
                ExpiresAt = expiry
            };
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
