﻿// <auto-generated />
using System;
using MatchesData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MatchesData.Migrations
{
    [DbContext(typeof(MatchesDbContext))]
    [Migration("20190611212306_InitialMatchesMigration")]
    partial class InitialMatchesMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MatchesData.Entities.Match", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("BestOf");

                    b.Property<int>("BlueScore")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RedScore")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<DateTime>("StartDateTime");

                    b.HasKey("Id");

                    b.ToTable("Match");
                });

            modelBuilder.Entity("MatchesData.Entities.MatchParticipation", b =>
                {
                    b.Property<int>("MatchId");

                    b.Property<string>("Side");

                    b.Property<int>("TeamId");

                    b.HasKey("MatchId", "Side");

                    b.HasIndex("TeamId");

                    b.HasIndex("MatchId", "TeamId")
                        .IsUnique();

                    b.ToTable("MatchParticipation");
                });

            modelBuilder.Entity("MatchesData.Entities.Team", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("LogoUrl")
                        .HasMaxLength(256);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Team");
                });

            modelBuilder.Entity("MatchesData.Entities.MatchParticipation", b =>
                {
                    b.HasOne("MatchesData.Entities.Match", "Match")
                        .WithMany("Participations")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MatchesData.Entities.Team", "Team")
                        .WithMany("MatchParticipations")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}