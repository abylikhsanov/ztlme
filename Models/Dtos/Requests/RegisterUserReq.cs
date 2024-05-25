using Microsoft.AspNetCore.Mvc;

namespace ztlme.Dtos;

public class RegisterUserReq
{
    [FromQuery(Name = "sessionId")]
    public string SessionId { get; set; }
}