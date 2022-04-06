// <copyright file="AppDbContextFactory.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RafBot.Persistence;

/// <summary>
/// Database context factory.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates the database context.
    /// </summary>
    /// <param name="args">Database context configuration arguments.</param>
    /// <returns>The configured database context.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=blog.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}