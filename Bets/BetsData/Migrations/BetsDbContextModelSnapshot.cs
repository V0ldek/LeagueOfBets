﻿// <auto-generated />
using System;
using BetsData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BetsData.Migrations
{
    [DbContext(typeof(BetsDbContext))]
    partial class BetsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BetsData.Entities.Bet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("Amount");

                    b.Property<int>("StakeId");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("StakeId");

                    b.ToTable("Bet");
                });

            modelBuilder.Entity("BetsData.Entities.Match", b =>
                {
                    b.Property<int>("Id");

                    b.Property<bool>("IsFinished")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<int?>("LosersScore");

                    b.Property<string>("WinningSide");

                    b.HasKey("Id");

                    b.ToTable("Match");
                });

            modelBuilder.Entity("BetsData.Entities.Stake", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("LosersScore");

                    b.Property<int>("MatchId");

                    b.Property<float>("Ratio");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIME()");

                    b.Property<string>("WinningSide")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.ToTable("Stake");
                });

            modelBuilder.Entity("BetsData.Entities.Bet", b =>
                {
                    b.HasOne("BetsData.Entities.Stake", "Stake")
                        .WithMany("Bets")
                        .HasForeignKey("StakeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BetsData.Entities.Stake", b =>
                {
                    b.HasOne("BetsData.Entities.Match", "Match")
                        .WithMany("Stakes")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
