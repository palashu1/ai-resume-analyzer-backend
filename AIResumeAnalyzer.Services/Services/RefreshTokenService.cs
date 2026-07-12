using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Services
{
    public class RefreshTokenService:IRefreshTokenService
    {
        private readonly JwtSettingsDto _jwtSettingsDto;
        public RefreshTokenService(IOptions<JwtSettingsDto> options) 
        {
            _jwtSettingsDto = options.Value;
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        public string HashRefreshToken(string refreshToken)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(refreshToken);

            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToBase64String(hash);
        }

        public bool VerifyRefreshToken(string refreshToken, string storedHash)
        {
            string hashedToken = HashRefreshToken(refreshToken);

            return hashedToken == storedHash;
        }

        public DateTime GetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(
                _jwtSettingsDto.RefreshTokenExpiryDays);
        }
    }
}
