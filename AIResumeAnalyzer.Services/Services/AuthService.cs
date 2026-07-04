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
    public class AuthService:IAuthService
    {
        private readonly ApplicationDbContext _db;
        public AuthService(ApplicationDbContext db) 
        { 
            _db = db;
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

            var existingUser = await _db.Users.AsNoTracking().AnyAsync(u=>u.Email== dto.Email);
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
                FirstName=dto.FirstName,
                LastName=dto.LastName,
                Email=dto.Email,
                PasswordHash= BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt=DateTime.UtcNow,
                UpdatedAt=DateTime.UtcNow,
                IsActive=true
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var response = new RegisterDto
            {
                Id=user.Id,
                Email=user.Email,
            };

            return new ApiResponseContainer<RegisterDto>
            {
                Success = true,
                Message="User registered successfully.",
                Data= response
            };

        }
    }
}
