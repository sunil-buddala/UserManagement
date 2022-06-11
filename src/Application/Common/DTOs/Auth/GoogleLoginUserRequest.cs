using Application.Common.SwaggerSchemaFilters.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Common.DTOs.Auth;

[SwaggerSchemaFilter(typeof(GoogleLoginUserDtoSchemaFilter))]
[SwaggerSchema(Required = new[] { "User" })]
public class GoogleLoginUserRequest
{
    [SwaggerSchema(Required = new[] { "Google id token" })]
    public string ExternalAccessToken { get; set; }
}