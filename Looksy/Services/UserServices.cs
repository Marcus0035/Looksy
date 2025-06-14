using Looksy.Infrastructure.Data.Models;

namespace Looksy.Services
{
    public class UserServices
    {
        private readonly LooksyDbContext _context;
        public UserServices(LooksyDbContext context)
        {
            _context = context;
        }

    }
}
