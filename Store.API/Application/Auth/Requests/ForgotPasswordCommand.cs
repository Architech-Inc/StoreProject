using Store.API.Application.Abstractions;
using Store.Models.DTOs.Auth;

namespace Store.API.Application.Auth.Requests;

public class ForgotPasswordCommand : IRequest<bool>
{
    public ForgotPasswordRequest Request { get; }

    public ForgotPasswordCommand(ForgotPasswordRequest request)
    {
        Request = request;
    }
}
