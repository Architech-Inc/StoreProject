using Store.API.Application.Abstractions;
using Store.Models.DTOs.Auth;

namespace Store.API.Application.Auth.Requests;

public class ConfirmPasswordResetCommand : IRequest<bool>
{
    public ConfirmPasswordResetRequest Request { get; }

    public ConfirmPasswordResetCommand(ConfirmPasswordResetRequest request)
    {
        Request = request;
    }
}
