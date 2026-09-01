using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

using Microsoft.AspNetCore.RateLimiting;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control")]
[EnableRateLimiting("PortalAuth")]
public class PortalAuthController : ControllerBase
{
    private readonly IPortalAuthService _authService;
    private readonly ILogger<PortalAuthController> _logger;

    public PortalAuthController(IPortalAuthService authService, ILogger<PortalAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterPortalAccountRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid registration data."));
        }

        try
        {
            var result = await _authService.RegisterAsync(request, ct);
            return StatusCode(201, ApiResponse<PortalAuthResponse>.Ok(result, "Account created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering portal account for {Email}", request.Email);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating your account."));
        }
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginPortalAccountRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid credentials provided."));
        }

        var result = await _authService.LoginAsync(request, ct);
        if (result == null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));
        }

        return Ok(ApiResponse<PortalAuthResponse>.Ok(result, "Login successful."));
    }

    [HttpGet("slugs/check")]
    public async Task<IActionResult> CheckSlug([FromQuery] string slug, CancellationToken ct)
    {
        var result = await _authService.CheckSlugAvailabilityAsync(slug, ct);
        return Ok(ApiResponse<SlugCheckResponse>.Ok(result));
    }

    [HttpPost("auth/link-tenant")]
    public async Task<IActionResult> LinkTenant([FromBody] LinkAccountTenantRequest request, CancellationToken ct)
    {
        await _authService.LinkAccountToTenantAsync(request.AccountId, request.TenantId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Account linked to tenant."));
    }
}

public record LinkAccountTenantRequest(Guid AccountId, Guid TenantId);
