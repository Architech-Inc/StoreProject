using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Ports;
using Store.API.Application.Auth.Requests;
using Store.Models.DTOs.Auth;

namespace Store.API.Application.Auth.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse?>
{
    private readonly IAuthPort _authPort;

    public LoginHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public Task<LoginResponse?> HandleAsync(LoginCommand request, CancellationToken ct = default)
        => _authPort.LoginAsync(request.Request, ct);
}

public class LoginWithEmailHandler : IRequestHandler<LoginWithEmailCommand, LoginResponse?>
{
    private readonly IAuthPort _authPort;

    public LoginWithEmailHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public Task<LoginResponse?> HandleAsync(LoginWithEmailCommand request, CancellationToken ct = default)
        => _authPort.LoginWithEmailAsync(request.Request, ct);
}

public class LoginWithPhoneHandler : IRequestHandler<LoginWithPhoneCommand, LoginResponse?>
{
    private readonly IAuthPort _authPort;

    public LoginWithPhoneHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public Task<LoginResponse?> HandleAsync(LoginWithPhoneCommand request, CancellationToken ct = default)
        => _authPort.LoginWithPhoneAsync(request.Request, ct);
}

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse?>
{
    private readonly IAuthPort _authPort;

    public RefreshTokenHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public Task<LoginResponse?> HandleAsync(RefreshTokenCommand request, CancellationToken ct = default)
        => _authPort.RefreshTokenAsync(request.Request, ct);
}

public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthPort _authPort;

    public LogoutHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public async Task<bool> HandleAsync(LogoutCommand request, CancellationToken ct = default)
    {
        await _authPort.LogoutAsync(request.UserId, ct);
        return true;
    }
}

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IAuthPort _authPort;

    public ResetPasswordHandler(IAuthPort authPort)
    {
        _authPort = authPort;
    }

    public Task<bool> HandleAsync(ResetPasswordCommand request, CancellationToken ct = default)
        => _authPort.ResetPasswordAsync(request.Request, ct);
}
