using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Wrappers;
using Domain.Entities;
using Domain.Exceptions;
using Forbids;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Auth;

public record LogOutCommand : IRequestWrapper<Unit>;

public class LogOutCommandHandler : IHandlerWrapper<LogOutCommand,Unit>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<User> _signInManager;
    private readonly IUsersDbReadOnlyContext _readOnlyContext;
    private readonly IUsersDbWriteContext writeContext;
    private readonly IForbid _forbid;

    public LogOutCommandHandler(IHttpContextAccessor httpContextAccessor,
        SignInManager<User> signInManager,
        IUsersDbReadOnlyContext readOnlyContext,
        IUsersDbWriteContext writeContext,
        IForbid forbid)
    {
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
        _readOnlyContext = readOnlyContext;
        this.writeContext = writeContext;
        _forbid = forbid;
    }

    public async Task<IResponse<Unit>> Handle(LogOutCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue("id");
        _forbid.NullOrEmpty(userId, UserNotFoundException.Instance);
        await _signInManager.SignOutAsync();
        var refreshTokens = await _readOnlyContext.RefreshTokens
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        writeContext.RefreshTokens.RemoveRange(refreshTokens);
        await writeContext.SaveChangesAsync(cancellationToken);
        return Response.Success(Unit.Value);
    }
}