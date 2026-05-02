using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Requests;

namespace Store.API.Application.Auth.Validators;

public class LoginCommandValidator : IRequestValidator<LoginCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(LoginCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Request.Username)) errors.Add("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Request.Password)) errors.Add("Password is required.");
        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}

public class LoginWithEmailCommandValidator : IRequestValidator<LoginWithEmailCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(LoginWithEmailCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Request.Email)) errors.Add("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Request.Password)) errors.Add("Password is required.");
        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}

public class LoginWithPhoneCommandValidator : IRequestValidator<LoginWithPhoneCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(LoginWithPhoneCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Request.Phone)) errors.Add("Phone is required.");
        if (string.IsNullOrWhiteSpace(request.Request.Password)) errors.Add("Password is required.");
        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}

public class RefreshTokenCommandValidator : IRequestValidator<RefreshTokenCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(RefreshTokenCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Request.RefreshToken)) errors.Add("RefreshToken is required.");
        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}
