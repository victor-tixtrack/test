using Microsoft.AspNetCore.Mvc;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;

namespace SmsService.Api.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController(ISmsProvider smsProvider) : ControllerBase
{
    [HttpPost("send")]
    public Task<SmsResponse> SendSms(SmsRequest request)
    {
        return smsProvider.SendSmsAsync(request);
    }
}
