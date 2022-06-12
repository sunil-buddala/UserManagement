using Application.Common.SwaggerSchemaFilters.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Common.DTOs.Auth;

[SwaggerSchemaFilter(typeof(PhoneLoginUserDtoSchemaFilter))]
public class PhoneLoginUserRequest
{
    [SwaggerSchema(Required = new[] { "The User Phone" })]
    public string Phone { get; set; }
}
