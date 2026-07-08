using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.Infrastructure.Entities;
using AIResumeAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Services
{
    public class JwtService:IJwtService
    {
        private readonly JwtSettingsDto _jwtSettingsDto;
        public JwtService(IOptions<JwtSettingsDto> options) 
        {
           _jwtSettingsDto = options.Value;
        }

        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettingsDto.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                _jwtSettingsDto.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                new Claim(JwtRegisteredClaimNames.Email, user.Email),

                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),

                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),

                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
               issuer: _jwtSettingsDto.Issuer,
               audience: _jwtSettingsDto.Audience,
               claims: claims,
               expires: expires,
               signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetAccessTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(
                _jwtSettingsDto.AccessTokenExpiryMinutes);
        }
    }
}
