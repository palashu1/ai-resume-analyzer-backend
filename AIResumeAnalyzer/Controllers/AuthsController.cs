using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.Infrastructure.Data;
using AIResumeAnalyzer.Model;
using AIResumeAnalyzer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpGet("check")]
        public async Task<IActionResult> check()
        {
            return Ok("Running good");
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            RegisterDto dto = new RegisterDto()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
            };
            var result=await _authService.RegisterAsync(dto);
            if(!result.Success)return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> login(LoginRequestModel model)
        {
            LoginRequestDto dto = new LoginRequestDto
            {
                email=model.email,
                password=model.password
            };

            var result = await _authService.LoginAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
