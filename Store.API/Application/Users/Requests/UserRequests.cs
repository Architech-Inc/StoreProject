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
