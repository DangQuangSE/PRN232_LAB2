using PRN232.LMSSystem.Repositories.Entities;

namespace PRN232.LMSSystem.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
