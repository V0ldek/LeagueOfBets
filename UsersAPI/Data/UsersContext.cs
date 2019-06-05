using Microsoft.EntityFrameworkCore;
using UsersAPI.Model;

namespace UsersAPI.Data
{
    public sealed partial class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
    }
}
