using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using ztlme.Models;
using ztlme.Services;

namespace ztlme.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignatureController : ControllerBase
{

    private readonly ISignatureService _signatureService;
    public SignatureController(ISignatureService signatureService)
    {
        _signatureService = signatureService;
    }

    [HttpPost("success")]
    public async Task<ActionResult<bool>> GetSuccess([FromBody] JsonDocument? data)
    {

        if (data == null)
        {
            return Ok();
        }
        var webhookData = data.RootElement;

        if (!webhookData.TryGetProperty("event", out JsonElement eventElement) && eventElement.ValueKind == JsonValueKind.String)
        {
            return Ok();
        }
        
        var eventToken = eventElement.GetString();
        if (eventToken != "SIGNATORY_SIGNED")
        {
            return Ok();
        }
        
        if (!webhookData.TryGetProperty("signatureOrderId", out JsonElement signatureOrderIdElement) && eventElement.ValueKind == JsonValueKind.String)
        {
            return Ok();
        }

        var signatureOrderId = signatureOrderIdElement.ToString();
     
        var response = await _signatureService.SignatureSuccess(eventToken, signatureOrderId);
        return Ok(response.Message);
    }

    [Authorize]
    [HttpGet("sign")]
    public async Task<ActionResult<ServiceResponse<string>>> SignDocument()
    {
        try
        {
            // Use controller to sign the document and upload to the database
            var response = await _signatureService.SignDocument();
            if (response.Success)
            {
                return Redirect(response.Data!);
            }

            return StatusCode(500, "Internal server error: " + response.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
    
}