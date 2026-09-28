using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.Common;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

/// <summary>
/// MT-07 — anonymous-facing tenant status endpoint.
///
/// Used by the public <c>/Status/{slug}</c> page on the Tenant Portal.
/// Intentionally has NO [Authorize] — anyone with a slug can check whether
/// a tenant is up, but the response body is sanitized by
/// <see cref="TenantOrchestrator.GetPublicStatusAsync"/> to never leak
/// secrets, connection strings, or operator notes.
/// </summary>
[ApiController]
[Route("api/public/tenants")]
[AllowAnonymous]
public class PublicStatusController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;
    private readonly ILogger<PublicStatusController> _logger;

    public PublicStatusController(
        ITenantOrchestrator orchestrator,
        ILogger<PublicStatusController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/public/tenants/{slug}/status
    ///
    /// Returns the tenant's public status payload: name, healthy flag,
    /// active + upcoming + recent maintenance windows.
    /// 404 if the slug does not exist.
    /// </summary>
    [HttpGet("{slug}/status")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetStatus(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(ApiResponse<object>.Fail("Slug is required."));
        }

        try
        {
            var dto = await _orchestrator.GetPublicStatusAsync(slug, ct);
            if (dto is null)
            {
                return NotFound(ApiResponse<object>.Fail($"No tenant found for slug '{slug}'."));
            }
            return Ok(ApiResponse<TenantStatusDto>.Ok(dto));
        }
        catch (Exception ex)
        {
            // Public endpoint — never leak stack traces. Always return a
            // minimal payload and let the operator UI retry.
            _logger.LogError(ex, "Public status lookup failed for slug {Slug}", slug);
            return StatusCode(503, ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Public status lookup")));
        }
    }
}