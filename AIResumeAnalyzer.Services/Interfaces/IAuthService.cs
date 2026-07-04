using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.DTO.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponseContainer<RegisterDto>> RegisterAsync(RegisterDto dto);
    }
}
