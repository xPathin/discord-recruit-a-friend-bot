// <copyright file="AppDbContext.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RafBot.Persistence.Models;

namespace RafBot.Persistence;

/// <summary>
/// EFCore application database context.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    protected AppDbContext()
    {
    }

    /// <summary>
    /// Gets the invite codes.
    /// </summary>
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();

    /// <summary>
    /// Gets the guild settings.
    /// </summary>
    public DbSet<GuildSetting> GuildSettings => Set<GuildSetting>();

    /// <summary>
    /// Gets the invited users.
    /// </summary>
    public DbSet<UserInvite> UserInvites => Set<UserInvite>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildSetting>().HasKey(nameof(GuildSetting.GuildId), nameof(GuildSetting.Key));
        modelBuilder.Entity<UserInvite>().HasKey(nameof(UserInvite.GuildId), nameof(UserInvite.InvitedUserId));
    }
}