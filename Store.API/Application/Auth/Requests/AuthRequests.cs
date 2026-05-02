using Store.API.Application.Abstractions;
using Store.Models.DTOs.Auth;

namespace Store.API.Application.Auth.Requests;

public record LoginCommand(LoginRequest Request) : IRequest<LoginResponse?>;

public record LoginWithEmailCommand(LoginWithEmailRequest Request) : IRequest<LoginResponse?>;

public record LoginWithPhoneCommand(LoginWithPhoneRequest Request) : IRequest<LoginResponse?>;

public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<LoginResponse?>;

public record LogoutCommand(Guid UserId) : IRequest<bool>;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<bool>;
