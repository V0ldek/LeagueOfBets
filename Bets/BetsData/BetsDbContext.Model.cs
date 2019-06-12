using BetsData.Entities;
using Microsoft.EntityFrameworkCore;
using BetsData.EnumPropertyBuilderExtensions;

namespace BetsData
{
    public sealed partial class BetsDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Match>(
                entity =>
                {
                    entity.HasKey(m => m.Id);
                    entity.Property(m => m.Id)
                        .ValueGeneratedNever();

                    entity.Ignore(m => m.IsFinished);

                    entity.Ignore(m => m.WinningSide);

                    entity.Property(m => m.BlueScore)
                        .IsRequired();

                    entity.Property(m => m.RedScore)
                        .IsRequired();

                    entity.Property(m => m.BestOf)
                        .IsRequired();

                    entity.HasMany(m => m.Stakes)
                        .WithOne(s => s.Match)
                        .HasForeignKey(s => s.MatchId);

                    entity.ToTable("Match");
                });

            modelBuilder.Entity<Stake>(
                entity =>
                {
                    entity.HasKey(s => s.Id);

                    entity.Property(m => m.BlueScore)
                        .IsRequired();

                    entity.Property(m => m.RedScore)
                        .IsRequired();

                    entity.Property(s => s.Ratio)
                        .IsRequired();

                    entity.Property(s => s.Timestamp)
                        .HasDefaultValueSql("SYSDATETIME()");

                    entity.HasMany(s => s.Bets)
                        .WithOne(b => b.Stake)
                        .HasForeignKey(b => b.StakeId);

                    entity.ToTable("Stake");
                });

            modelBuilder.Entity<Bet>(
                entity =>
                {
                    entity.HasKey(b => b.Id);

                    entity.Property(b => b.UserId)
                        .HasMaxLength(256)
                        .IsRequired();

                    entity.Property(b => b.Amount)
                        .IsRequired();

                    entity.ToTable("Bet");
                });

            modelBuilder.Query<Account>(
                query =>
                {
                    query.Property(q => q.UserId)
                        .HasColumnName("UserId");

                    query.Property(q => q.Balance)
                        .HasColumnName("Balance");

                    query.ToView("View_Account");
                });

            modelBuilder.Query<AccountConfiguration>(
                query =>
                {
                    query.Property(q => q.BaseBalance)
                        .HasColumnName("BaseBalance");

                    query.ToView("View_AccountConfiguration");
                });
        }
    }
}