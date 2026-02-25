using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookingInfrastructure.Contracts.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BookingDbContext _context;

    public UserRepository(BookingDbContext context)
    {
        _context = context;
    }

    // IRepository<Users> methods
    public async Task<Users?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Users> Add(Users entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Users> Update(Users entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Users entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // IUserRepository-specific methods
    public async Task<Users?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Users?> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetById(id, cancellationToken);
    }

    public async Task<Users> AddUser(Users user, CancellationToken cancellationToken = default)
    {
        return await Add(user, cancellationToken);
    }

    public async Task<Users> UpdateUser(Users user, CancellationToken cancellationToken = default)
    {
        return await Update(user, cancellationToken);
    }

    public async Task<Users> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetById(id, cancellationToken);
        if (user != null)
        {
            await Delete(user, cancellationToken);
        }
        return user!;
    }
}