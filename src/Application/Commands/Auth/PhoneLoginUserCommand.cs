using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs.Auth;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Wrappers;
using Domain.Entities;
using Domain.Exceptions;
using Forbids;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.Auth;

public record PhoneLoginUserCommand(PhoneLoginUserRequest LoginUserRequest) : IRequestWrapper<AuthenticateResponse>;

public class PhoneLoginUserCommandHandler : IHandlerWrapper<PhoneLoginUserCommand, AuthenticateResponse>
{
    private readonly IAuthenticateService _authenticateService;
    private readonly IForbid _forbid;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public PhoneLoginUserCommandHandler(UserManager<User> userManager,
        SignInManager<User> signInManager,
        IAuthenticateService authenticateService,
        IForbid forbid)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authenticateService = authenticateService;
        _forbid = forbid;
    }

    public async Task<IResponse<AuthenticateResponse>> Handle(PhoneLoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            PhoneNumber = request.LoginUserRequest.Phone,
            UserName = request.LoginUserRequest.Phone,
            PhoneNumberConfirmed = true
        };

        var existingUser = await _userManager.FindByNameAsync(request.LoginUserRequest.Phone);
        if (existingUser == null)
        {
            var createResult = await _userManager.CreateAsync(user);
            _forbid.False(createResult.Succeeded, RegisterException.Instance);
        }
        else
        {
            user = existingUser;
        }

        await _signInManager.SignInAsync(user, false);
        return Response.Success(await _authenticateService.Authenticate(user, cancellationToken));
    }
}
