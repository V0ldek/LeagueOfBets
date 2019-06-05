using Microsoft.EntityFrameworkCore;
using UsersAPI.Model;

namespace UsersAPI.Data
{
    public sealed partial class UsersContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) => 
            modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable("User"); });
    }
}