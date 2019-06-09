using MatchesData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MatchesData
{
    public sealed partial class MatchesDbContext : DbContext
    {
        public MatchesDbContext(string connectionString) : this(
            new DbContextOptionsBuilder<MatchesDbContext>()
                .UseSqlServer(connectionString)
                .Options)
        {
        }

        public MatchesDbContext(DbContextOptions<MatchesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Match> Matches { get; set; }

        public DbSet<Team> Teams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.ConfigureWarnings(
                warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
    }
}