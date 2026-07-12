using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Interfaces
{
    public interface IRequestInfoService
    {
        string GetUserAgent();
        string GetIpAddress();
    }
}
