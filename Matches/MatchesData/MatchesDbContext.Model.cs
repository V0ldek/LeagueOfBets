using System;
using MatchesData.Entities;
using MatchesData.Entities.Enums;
using MatchesData.EnumPropertyBuilderExtensions;
using Microsoft.EntityFrameworkCore;

namespace MatchesData
{
    public sealed partial class MatchesDbContext
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

                    entity.Property(m => m.StartDateTime)
                        .IsRequired();

                    entity.Property(m => m.Format)
                        .HasStringConversion()
                        .IsRequired();

                    entity.Property(m => m.BlueScore)
                        .HasDefaultValue(0)
                        .IsRequired();

                    entity.Property(m => m.RedScore)
                        .HasDefaultValue(0)
                        .IsRequired();

                    entity.Property(m => m.IsFinished)
                        .HasDefaultValue(false)
                        .IsRequired();

                    entity.HasMany(m => m.Participations)
                        .WithOne(p => p.Match)
                        .HasForeignKey(p => p.MatchId)
                        .IsRequired();

                    entity.ToTable("Match");
                });

            modelBuilder.Entity<Team>(
                entity =>
                {
                    entity.HasKey(t => t.Id);
                    entity.Property(t => t.Id)
                        .ValueGeneratedNever();

                    entity.Property(t => t.Name)
                        .HasMaxLength(256)
                        .IsRequired();

                    entity.Property(t => t.LogoUrl)
                        .HasMaxLength(256)
                        .IsRequired(false);

                    entity.HasMany(t => t.MatchParticipations)
                        .WithOne(p => p.Team)
                        .HasForeignKey(p => p.TeamId)
                        .IsRequired();

                    entity.HasIndex(t => t.Name)
                        .IsUnique();

                    entity.ToTable("Team");
                });

            modelBuilder.Entity<MatchParticipation>(
                entity =>
                {
                    entity.HasKey(t => new {t.MatchId, t.Side});

                    entity.Property(p => p.Side)
                        .HasStringConversion()
                        .IsRequired();

                    entity.HasIndex(t => new {t.MatchId, t.TeamId})
                        .IsUnique();

                    entity.ToTable("MatchParticipation");
                });
        }
    }
}