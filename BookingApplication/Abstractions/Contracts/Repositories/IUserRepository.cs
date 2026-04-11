using BookingDomain.Entities;
using BookingDomain.Repositories;

namespace BookingApplication.Abstractions.Contracts.Repositories;

public interface IUserRepository : IRepository<Users>{
    Task<Users?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
    Task<Users?> GetUserById(Guid id, CancellationToken cancellationToken = default);
    Task<Users> AddUser(Users user, CancellationToken cancellationToken = default);
    Task<Users> UpdateUser(Users user, CancellationToken cancellationToken = default);
    Task<Users> DeleteUser(Guid id, CancellationToken cancellationToken = default);
    Task<List<Users>> GetAllUsers(CancellationToken cancellationToken = default);
    Task<bool> SetUserActiveStatus(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
