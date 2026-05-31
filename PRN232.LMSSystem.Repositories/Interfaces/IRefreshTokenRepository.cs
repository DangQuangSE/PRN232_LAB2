using PRN232.LMSSystem.Repositories.Entities;

namespace PRN232.LMSSystem.Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}
