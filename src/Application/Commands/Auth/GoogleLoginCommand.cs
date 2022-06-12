using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs.Auth;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Settings;
using Application.Common.Wrappers;
using Domain.Entities;
using Forbids;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.Auth;

public record GoogleLoginCommand(GoogleLoginUserRequest LoginUserRequest) : IRequestWrapper<AuthenticateResponse>;

public class GoogleLoginCommandHandler : IHandlerWrapper<GoogleLoginCommand, AuthenticateResponse>
{
    private readonly IAuthenticateService _authenticateService;
    private readonly IForbid _forbid;
    private readonly JwtSettings jwtSettings;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private const string GoogleProviderKey = "Google";

    public GoogleLoginCommandHandler(UserManager<User> userManager, SignInManager<User> signInManager,
        IAuthenticateService authenticateService, IForbid forbid, JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authenticateService = authenticateService;
        _forbid = forbid;
        this.jwtSettings = jwtSettings;
    }

    public async Task<IResponse<AuthenticateResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.LoginUserRequest.ExternalAccessToken,
               new GoogleJsonWebSignature.ValidationSettings()
               {
                   Audience = new[] { jwtSettings.Google.ClientId }
               });

        var user = await _userManager.FindByLoginAsync(GoogleProviderKey, payload.Subject);
        if (user == null)
        {
            user = new User()
            {
                Email = payload.Email,
                UserName = payload.Email
            };

            await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProviderKey, payload.Subject, GoogleProviderKey.ToUpperInvariant()));
        }

        await _signInManager.SignInAsync(user, false);
        return Response.Success(await _authenticateService.Authenticate(user, cancellationToken));
    }
}