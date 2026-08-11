using Store.API.Application.Abstractions;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.API.Application.Users.Requests;

public record GetUsersQuery(PagedRequest Request) : IRequest<PagedResult<UserDto>>;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserDto>;

public record UpdateUserCommand(Guid UserId, UpdateUserRequest Request) : IRequest<UserDto?>;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;

public record ChangeUserPasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<bool>;

public record GetUserAvatarQuery(string Username) : IRequest<string?>;

public record IssueTempPasswordCommand(Guid UserId) : IRequest<string>;

public record UpdateUserContactsCommand(Guid UserId, UpdateUserContactsRequest Request) : IRequest<bool>;

public record Enable2FACommand(Guid UserId) : IRequest<Enable2FAResponse>;
public record Verify2FACommand(Guid UserId, Verify2FARequest Request) : IRequest<bool>;
public record GetRecentActivityQuery(Guid UserId) : IRequest<IReadOnlyCollection<AuditLogDto>>;
