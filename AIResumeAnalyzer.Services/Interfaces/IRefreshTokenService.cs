using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();

        string HashRefreshToken(string refreshToken);

        bool VerifyRefreshToken(string refreshToken, string storedHash);

        DateTime GetRefreshTokenExpiry();
    }
}
