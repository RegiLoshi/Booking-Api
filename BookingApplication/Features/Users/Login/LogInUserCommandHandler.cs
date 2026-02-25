// using MediatR;
// using BookingDomain;
// using BookingDomain.Repositories;
// using UserEntity = BookingDomain.Users;
// using BCrypt.Net;
// using FluentValidation;
//
// namespace BookingApplication.Features.Users.Login
// {
//     public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, Guid>
//     {
//         private readonly IRepository<UserEntity> _userRepository;
//
//         public LogInUserCommandHandler(IRepository<UserEntity> userRepository)
//         {
//             _userRepository = userRepository;
//         }
//
//         public async Task<Guid> Handle(LogInUserCommand request, CancellationToken cancellationToken)
//         {
//             
//         }
//     }
// }