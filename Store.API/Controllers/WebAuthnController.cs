using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.Interfaces.Services;
using Fido2NetLib;
using Store.Models.DTOs.Common;
using System.Security.Claims;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebAuthnController : ControllerBase
{
    private readonly IWebAuthnService _webAuthnService;

    public WebAuthnController(IWebAuthnService webAuthnService)
    {
        _webAuthnService = webAuthnService;
    }

    [Authorize]
    [HttpPost("makeCredentialOptions")]
    public async Task<IActionResult> MakeCredentialOptions(CancellationToken ct)
    {
        var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        var options = await _webAuthnService.RequestNewCredentialAsync(userId, ct);
        return Ok(options);
    }

    [Authorize]
    [HttpPost("makeCredential")]
    public async Task<IActionResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse response, CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var result = await _webAuthnService.RegisterNewCredentialAsync(userId, response, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public class AssertionOptionsRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("assertionOptions")]
    public async Task<IActionResult> AssertionOptions([FromBody] AssertionOptionsRequest request, CancellationToken ct)
    {
        try
        {
            var options = await _webAuthnService.RequestAssertionAsync(request.Username, ct);
            return Ok(options);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("makeAssertion")]
    public async Task<IActionResult> MakeAssertion(
        [FromBody] AuthenticatorAssertionRawResponse response, 
        [FromServices] Store.API.Application.Auth.Ports.IAuthPort authPort,
        CancellationToken ct)
    {
        try
        {
            var (result, userId) = await _webAuthnService.MakeAssertionAsync(response, ct);
            
            var loginResponse = await authPort.LoginWithBiometricsAsync(userId, ct);
            if (loginResponse == null) return Unauthorized(new { message = "Biometric authentication succeeded, but account is invalid" });

            // Mimic the normal Auth login response JSON format for compatibility
            return Ok(new { success = true, data = loginResponse });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("credentials")]
    public async Task<IActionResult> GetCredentials(CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var credentials = await _webAuthnService.GetCredentialsAsync(userId, ct);
            return Ok(Store.Models.DTOs.Common.ApiResponse<List<Store.Models.DTOs.Auth.FidoCredentialDto>>.Ok(credentials));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("credentials/{id}")]
    public async Task<IActionResult> DeleteCredential(int id, CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var success = await _webAuthnService.RemoveCredentialAsync(userId, id, ct);
            if (!success)
                return NotFound(new { message = "Credential not found" });

            return Ok(Store.Models.DTOs.Common.ApiResponse<object>.Ok(null!, "Credential removed."));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
