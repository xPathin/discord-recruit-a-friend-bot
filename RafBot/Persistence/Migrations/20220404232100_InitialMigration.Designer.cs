﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RafBot.Persistence;

#nullable disable

namespace RafBot.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20220404232100_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("RafBot.Persistence.Models.GuildSetting", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId", "Key");

                    b.ToTable("GuildSettings");
                });

            modelBuilder.Entity("RafBot.Persistence.Models.InviteCode", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Uses")
                        .HasColumnType("INTEGER");

                    b.HasKey("Code");

                    b.ToTable("InviteCodes");
                });

            modelBuilder.Entity("RafBot.Persistence.Models.UserInvite", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("InvitedUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InviteCodeId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("JoinDate")
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId", "InvitedUserId");

                    b.HasIndex("InviteCodeId");

                    b.ToTable("UserInvites");
                });

            modelBuilder.Entity("RafBot.Persistence.Models.UserInvite", b =>
                {
                    b.HasOne("RafBot.Persistence.Models.InviteCode", "InviteCode")
                        .WithMany()
                        .HasForeignKey("InviteCodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InviteCode");
                });
#pragma warning restore 612, 618
        }
    }
}