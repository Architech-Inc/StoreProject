using Store.API.Application.Abstractions;
using Store.API.Application.Users.Ports;
using Store.API.Application.Users.Requests;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.API.Application.Users.Handlers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUsersPort _usersPort;

    public GetUsersHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<PagedResult<UserDto>> HandleAsync(GetUsersQuery request, CancellationToken ct = default)
        => _usersPort.GetAllAsync(request.Request, ct);
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUsersPort _usersPort;

    public GetUserByIdHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<UserDto?> HandleAsync(GetUserByIdQuery request, CancellationToken ct = default)
        => _usersPort.GetByIdAsync(request.UserId, ct);
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUsersPort _usersPort;

    public CreateUserHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<UserDto> HandleAsync(CreateUserCommand request, CancellationToken ct = default)
        => _usersPort.CreateAsync(request.Request, ct);
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly IUsersPort _usersPort;

    public UpdateUserHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<UserDto?> HandleAsync(UpdateUserCommand request, CancellationToken ct = default)
        => _usersPort.UpdateAsync(request.UserId, request.Request, ct);
}

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUsersPort _usersPort;

    public DeleteUserHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<bool> HandleAsync(DeleteUserCommand request, CancellationToken ct = default)
        => _usersPort.DeleteAsync(request.UserId, ct);
}

public class ChangeUserPasswordHandler : IRequestHandler<ChangeUserPasswordCommand, bool>
{
    private readonly IUsersPort _usersPort;

    public ChangeUserPasswordHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<bool> HandleAsync(ChangeUserPasswordCommand request, CancellationToken ct = default)
        => _usersPort.ChangePasswordAsync(request.UserId, request.Request, ct);
}

public class GetUserAvatarHandler : IRequestHandler<GetUserAvatarQuery, string?>
{
    private readonly IUsersPort _usersPort;

    public GetUserAvatarHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<string?> HandleAsync(GetUserAvatarQuery request, CancellationToken ct = default)
        => _usersPort.GetAvatarAsync(request.Username, ct);
}

public class IssueTempPasswordHandler : IRequestHandler<IssueTempPasswordCommand, string>
{
    private readonly IUsersPort _usersPort;

    public IssueTempPasswordHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<string> HandleAsync(IssueTempPasswordCommand request, CancellationToken ct = default)
        => _usersPort.IssueTempPasswordAsync(request.UserId, ct);
}

public class UpdateUserContactsHandler : IRequestHandler<UpdateUserContactsCommand, bool>
{
    private readonly IUsersPort _usersPort;

    public UpdateUserContactsHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<bool> HandleAsync(UpdateUserContactsCommand request, CancellationToken ct = default)
        => _usersPort.UpdateContactsAsync(request.UserId, request.Request, ct);
}

public class Enable2FAHandler : IRequestHandler<Enable2FACommand, Enable2FAResponse>
{
    private readonly IUsersPort _usersPort;

    public Enable2FAHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<Enable2FAResponse> HandleAsync(Enable2FACommand request, CancellationToken ct = default)
        => _usersPort.Enable2FAAsync(request.UserId, ct);
}

public class Verify2FAHandler : IRequestHandler<Verify2FACommand, bool>
{
    private readonly IUsersPort _usersPort;

    public Verify2FAHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<bool> HandleAsync(Verify2FACommand request, CancellationToken ct = default)
        => _usersPort.Verify2FAAsync(request.UserId, request.Request, ct);
}

public class GetRecentActivityHandler : IRequestHandler<GetRecentActivityQuery, IReadOnlyCollection<AuditLogDto>>
{
    private readonly IUsersPort _usersPort;

    public GetRecentActivityHandler(IUsersPort usersPort)
    {
        _usersPort = usersPort;
    }

    public Task<IReadOnlyCollection<AuditLogDto>> HandleAsync(GetRecentActivityQuery request, CancellationToken ct = default)
        => _usersPort.GetRecentActivityAsync(request.UserId, ct);
}
