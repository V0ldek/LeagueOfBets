using System.Linq;
using System.Threading.Tasks;
using BetsData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BetsData
{
    public sealed partial class BetsDbContext : DbContext
    {
        public DbSet<Bet> Bets { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<Stake> Stakes { get; set; }

        public DbQuery<Account> Accounts { get; set; }

        internal DbQuery<AccountConfiguration> AccountConfigurations { get; set; }

        public async Task<AccountConfiguration> GetAccountConfigurationAsync() => await AccountConfigurations.FirstOrDefaultAsync();

        public BetsDbContext(string connectionString) : this(
            new DbContextOptionsBuilder<BetsDbContext>()
                .UseSqlServer(connectionString)
                .Options)
        {
        }

        public BetsDbContext(DbContextOptions<BetsDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.ConfigureWarnings(
                warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
    }
}
