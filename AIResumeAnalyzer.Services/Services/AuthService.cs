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

namespace AIResumeAnalyzer.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        public AuthService(ApplicationDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
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
            var isUserExist = await _db.Users.FirstOrDefaultAsync(f => f.Email == dto.email);
            if (isUserExist == null)
            {
                return new ApiResponseContainer<LoginResponseDto>
                {
                    Success=false,
                    Message="Invalid user."
                };
            }
            bool isPasswordValid= BCrypt.Net.BCrypt.Verify(dto.password, isUserExist.PasswordHash);
            if (!isPasswordValid) 
            {
                return new ApiResponseContainer<LoginResponseDto>
                {
                    Success=false,
                    Message= "Invalid Password."
                };
            }

            //Jwt token creation

            string accessToken = _jwtService.GenerateAccessToken(isUserExist);
            LoginResponseDto loginResponse = new LoginResponseDto
            {
                Id = isUserExist.Id,
                FirstName = isUserExist.FirstName,
                LastName = isUserExist.LastName,
                Email = isUserExist.Email,
                AccessToken = accessToken,
                AccessTokenExpiresOn = _jwtService.GetAccessTokenExpiry()
            };

            return new ApiResponseContainer<LoginResponseDto>
            {
                Success=true,
                Message="Login successfully",
                Data= loginResponse,
            };
            

        }
    }
}
