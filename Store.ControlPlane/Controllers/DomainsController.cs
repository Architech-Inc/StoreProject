using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants/{id:guid}/domains")]
public class DomainsController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;
    private readonly ILogger<DomainsController> _logger;

    public DomainsController(ITenantOrchestrator orchestrator, ILogger<DomainsController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDomains(Guid id, CancellationToken ct)
    {
        var domainConfig = await _orchestrator.GetDomainConfigAsync(id, ct);
        if (domainConfig == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDomainDto>.Ok(domainConfig));
    }

    [HttpPost("custom")]
    public async Task<IActionResult> SetCustomDomain(Guid id, [FromBody] SetCustomDomainRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid domain name."));
        }

        try
        {
            var result = await _orchestrator.SetCustomDomainAsync(id, request.Domain, ct);
            return Ok(ApiResponse<TenantDomainDto>.Ok(result, "Custom domain registered. Add the DNS TXT record to verify ownership."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering custom domain for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to register custom domain."));
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCustomDomain(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _orchestrator.VerifyCustomDomainAsync(id, ct);
            return Ok(ApiResponse<VerifyDomainResponse>.Ok(result, result.Message ?? "Verification check completed."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying custom domain for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to perform DNS verification."));
        }
    }

    [HttpDelete("custom")]
    public async Task<IActionResult> RemoveCustomDomain(Guid id, CancellationToken ct)
    {
        var success = await _orchestrator.RemoveCustomDomainAsync(id, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Custom domain removed. Silo accessible via platform subdomain."));
    }
}
