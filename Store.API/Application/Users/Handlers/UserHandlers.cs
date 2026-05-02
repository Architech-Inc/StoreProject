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
