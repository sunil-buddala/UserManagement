using System.Threading;
using System.Threading.Tasks;
using API.Routes;
using Application.Commands.Auth;
using Application.Common.DTOs.Auth;
using Application.Common.Models;
using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Endpoints.Auth;

[Route(AuthRoutes.LoginWithGoogle)]
public class LoginWithGoogle : BaseAsyncEndpoint
    .WithRequest<GoogleLoginUserRequest>
    .WithResponse<IResponse<string>>
{
    private readonly IMediator _mediator;

    public LoginWithGoogle(IMediator mediator) => _mediator = mediator;

    [HttpPost,
     SwaggerOperation(Description = "Signs in or signs up provided google user and return token",
         Summary = "Sign In or Sing Up wiht Google Account",
         OperationId = "Auth.LoginWithGoogle",
         Tags = new[] { "Auth" }),
     SwaggerResponse(200, "User logged in successfully", typeof(IResponse<string>)),
     Produces("application/json"), Consumes("application/json")]
    public override async Task<ActionResult<IResponse<string>>> HandleAsync(
        [SwaggerRequestBody("User google login payload", Required = true)]
        GoogleLoginUserRequest loginUserRequest,
        CancellationToken cancellationToken = new()) =>
        Ok(await _mediator.Send(new GoogleLoginCommand(loginUserRequest), cancellationToken));
}