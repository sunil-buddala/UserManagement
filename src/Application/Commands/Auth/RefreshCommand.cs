﻿using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs.Auth;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Wrappers;
using Domain.Entities;
using Domain.Exceptions;
using Forbids;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Auth;

public record RefreshCommand(RefreshRequest RefreshRequest) : IRequestWrapper<AuthenticateResponse>;

public class RefreshCommandHandler : IHandlerWrapper<RefreshCommand,AuthenticateResponse>
{
    private readonly IAuthenticateService _authenticateService;
    private readonly IForbid _forbid;
    private readonly IRefreshTokenValidator _refreshTokenValidator;
    private readonly IUsersDbReadOnlyContext readOnlyContext;
    private readonly IUsersDbWriteContext writeContext;
    private readonly UserManager<User> _userManager;

    public RefreshCommandHandler(IRefreshTokenValidator refreshTokenValidator,
        IUsersDbReadOnlyContext readOnlyContext,
        IUsersDbWriteContext writeContext,
        UserManager<User> userManager,
        IAuthenticateService authenticateService,
        IForbid forbid)
    {
        _refreshTokenValidator = refreshTokenValidator;
        this.readOnlyContext = readOnlyContext;
        this.writeContext = writeContext;
        _userManager = userManager;
        _authenticateService = authenticateService;
        _forbid = forbid;
    }

    public async Task<IResponse<AuthenticateResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var refreshRequest = request.RefreshRequest;
        var isValidRefreshToken = _refreshTokenValidator.Validate(refreshRequest.RefreshToken);
        _forbid.False(isValidRefreshToken, InvalidRefreshTokenException.Instance);
        var refreshToken =
            await readOnlyContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshRequest.RefreshToken,
                cancellationToken);
        _forbid.Null(refreshToken, InvalidRefreshTokenException.Instance);
        writeContext.RefreshTokens.Remove(refreshToken);
        await writeContext.SaveChangesAsync(cancellationToken);
            
        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        _forbid.Null(user, UserNotFoundException.Instance);
        return Response.Success(await _authenticateService.Authenticate(user, cancellationToken));
    }
}