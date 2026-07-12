using AIResumeAnalyzer.DTO.Utility;
using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.Infrastructure.Data;
using AIResumeAnalyzer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AIResumeAnalyzer.Infrastructure.Entities;
using Azure.Core;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using System.Transactions;

namespace AIResumeAnalyzer.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestInfoService _requestInfoService;
        public AuthService(ApplicationDbContext db, IJwtService jwtService, IRefreshTokenService refreshTokenService,
            IHttpContextAccessor httpContextAccessor, IRequestInfoService requestInfoService)
        {
            _db = db;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _httpContextAccessor = httpContextAccessor;
            _requestInfoService = requestInfoService;
        }


        public async Task<ApiResponseContainer<RegisterDto>> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                return new ApiResponseContainer<RegisterDto>
                {
                    Success = false,
                    Message = "Password and Confirm Password do not match."
                };

            }

            var existingUser = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
            {
                return new ApiResponseContainer<RegisterDto>
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var response = new RegisterDto
            {
                Id = user.Id,
                Email = user.Email,
            };

            return new ApiResponseContainer<RegisterDto>
            {
                Success = true,
                Message = "User registered successfully.",
                Data = response
            };

        }
        public async Task<ApiResponseContainer<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            LoginResponseDto loginResponseDto = new LoginResponseDto();
            try
            {
                var isUserExist = await _db.Users.FirstOrDefaultAsync(f => f.Email == dto.email);
                if (isUserExist == null)
                {
                    return new ApiResponseContainer<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.password, isUserExist.PasswordHash);
                if (!isPasswordValid)
                {
                    return new ApiResponseContainer<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                //Jwt token creation

                string accessToken = _jwtService.GenerateAccessToken(isUserExist);
                string refreshToken = _refreshTokenService.GenerateRefreshToken();
                string hashedRefreshToken = _refreshTokenService.HashRefreshToken(refreshToken);
                DateTime expiry = _refreshTokenService.GetRefreshTokenExpiry();
                string userAgent = _requestInfoService.GetUserAgent();
                string ipAddress = _requestInfoService.GetIpAddress();


                RefreshToken refreshTokenEntity = new RefreshToken()
                {
                    UserId = isUserExist.Id,
                    TokenHash = hashedRefreshToken,
                    CreatedOn = DateTime.UtcNow,
                    ExpiresOn = expiry,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    isActive = true

                };

                await _db.RefreshTokens.AddAsync(refreshTokenEntity);
                await _db.SaveChangesAsync();


                loginResponseDto.Id = isUserExist.Id;
                loginResponseDto.FirstName = isUserExist.FirstName;
                loginResponseDto.LastName = isUserExist.LastName;
                loginResponseDto.Email = isUserExist.Email;
                loginResponseDto.AccessToken = accessToken;
                loginResponseDto.AccessTokenExpiresOn = _jwtService.GetAccessTokenExpiry();
                loginResponseDto.RefreshToken = refreshToken;

            }
            catch (Exception ex)
            {
                return new ApiResponseContainer<LoginResponseDto>
                {
                    Success = false,
                    Message = "Login failed.",
                    Data = loginResponseDto,
                };
            }


            return new ApiResponseContainer<LoginResponseDto>
            {
                Success = true,
                Message = "Login successfully",
                Data = loginResponseDto,
            };
        }
        public async Task<ApiResponseContainer<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            RefreshTokenResponseDto refreshTokenResponseDto = new RefreshTokenResponseDto();
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var convertedHash = _refreshTokenService.HashRefreshToken(refreshToken);
                var storeHash = await _db.RefreshTokens.Include(i => i.User).FirstOrDefaultAsync(f => f.TokenHash == convertedHash && f.isActive == true);
                if (storeHash == null)
                {
                    return new ApiResponseContainer<RefreshTokenResponseDto>()
                    {
                        Success = false,
                        Message = "Invalid refresh token."
                    };
                }

                if (storeHash.ExpiresOn <= DateTime.UtcNow)
                {
                    return new ApiResponseContainer<RefreshTokenResponseDto>
                    {
                        Success = false,
                        Message = "Refresh token has expired."
                    };
                }

                string accessToken = _jwtService.GenerateAccessToken(storeHash.User);
                DateTime expiry = _jwtService.GetAccessTokenExpiry();
                string newRefreshToken = _refreshTokenService.GenerateRefreshToken();
                string hashedRefreshToken = _refreshTokenService.HashRefreshToken(newRefreshToken);
                DateTime refreshExpiry = _refreshTokenService.GetRefreshTokenExpiry();
                string userAgent = _requestInfoService.GetUserAgent();
                string ipAddress = _requestInfoService.GetIpAddress();

                // update existing data
                storeHash.RevokedOn = DateTime.UtcNow;
                storeHash.isActive = false;
                //_db.RefreshTokens.Update(storeHash);
                // save new entity
                RefreshToken nRefreshToken = new RefreshToken()
                {
                    UserId = storeHash.UserId,
                    TokenHash = hashedRefreshToken,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    CreatedOn = DateTime.UtcNow,
                    ExpiresOn = refreshExpiry,
                    isActive = true
                };
                await _db.RefreshTokens.AddAsync(nRefreshToken);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                refreshTokenResponseDto.AccessToken = accessToken;
                refreshTokenResponseDto.AccessTokenExpiresOn = expiry;
                refreshTokenResponseDto.RefreshToken = newRefreshToken;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponseContainer<RefreshTokenResponseDto>()
                {
                    Success = false,
                    Message = "Something went wrong."
                };
            }

            return new ApiResponseContainer<RefreshTokenResponseDto>()
            {
                Success = true,
                Message = "Done.",
                Data = refreshTokenResponseDto
            };
        }
    }
}
