using Looksy.Models.DTOs;
using Looksy.Repositories;

namespace Looksy.Services
{
    public class UsersService
    {
        #region Const And Readonly Fields
        private readonly ILogger<UsersService> _logger;
        private readonly UsersRepository _usersRepository;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public UsersService(ILogger<UsersService> logger, UsersRepository repository)
        {
            _logger = logger;
            _usersRepository = repository;
        }
        #endregion

        #region Private

        #endregion

        #region Public
        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _usersRepository.GetById(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID {UserId}", userId);
                return null;
            }
        }
        #endregion
    }
}
