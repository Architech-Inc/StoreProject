using Store.API.Application.Abstractions;
using Store.API.Application.Users.Requests;

namespace Store.API.Application.Users.Validators;

public class GetUsersQueryValidator : IRequestValidator<GetUsersQuery>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(GetUsersQuery request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (request.Request.Page <= 0) errors.Add("Page must be greater than 0.");
        if (request.Request.PageSize <= 0) errors.Add("PageSize must be greater than 0.");
        if (request.Request.PageSize > 200) errors.Add("PageSize must not exceed 200.");
        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}

public class CreateUserCommandValidator : IRequestValidator<CreateUserCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(CreateUserCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Request.Username) || request.Request.Username.Trim().Length < 3)
            errors.Add("Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(request.Request.Email) || !request.Request.Email.Contains('@'))
            errors.Add("A valid email address is required.");

        if (string.IsNullOrWhiteSpace(request.Request.Password) || request.Request.Password.Length < 8)
            errors.Add("Password must be at least 8 characters.");

        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}

public class ChangeUserPasswordCommandValidator : IRequestValidator<ChangeUserPasswordCommand>
{
    public Task<IReadOnlyCollection<string>> ValidateAsync(ChangeUserPasswordCommand request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Request.CurrentPassword))
            errors.Add("CurrentPassword is required.");

        if (string.IsNullOrWhiteSpace(request.Request.NewPassword) || request.Request.NewPassword.Length < 8)
            errors.Add("NewPassword must be at least 8 characters.");

        if (!string.Equals(request.Request.NewPassword, request.Request.ConfirmPassword, StringComparison.Ordinal))
            errors.Add("ConfirmPassword must match NewPassword.");

        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}
