using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IUsersDbReadOnlyContext
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
