

using Looksy.Models;
using Looksy.Models.DTOs;
using System.Security.Claims;

namespace Looksy.Repositories
{
    public class UsersRepository
    {
        #region Const And Readonly Fields
        private readonly LooksyDbContext _context;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public UsersRepository(LooksyDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Private

        #endregion

        #region Public
        public async Task<UserDto?> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            };
        }

        #endregion

    }
}
