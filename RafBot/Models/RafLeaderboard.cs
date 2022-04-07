// <copyright file="RafLeaderboard.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace RafBot.Models;

/// <summary>
/// Recruit a friend leaderboard.
/// </summary>
public partial class RafLeaderboard
{
    /// <summary>
    /// Gets or sets the created date and time.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets the leaerboard entries.
    /// </summary>
    public List<RafLeaderboardEntry> LeaderboardEntries { get; } = new List<RafLeaderboardEntry>();

    /// <summary>
    /// Print the leaderboard.
    /// </summary>
    /// <returns>A formated discord message.</returns>
    public Embed PrintLeaderboard()
    {
        var sb = new StringBuilder();
        var eb = new EmbedBuilder
        {
            Title = "Recruit a Friend - Leaderboard",
            Color = Color.Green,
        };
        eb.WithCurrentTimestamp();

        var i = 0;
        foreach (var leaderboardEntry in LeaderboardEntries.OrderByDescending(x => x.Invites).ThenByDescending(x => x.PendingInvites).Take(10))
        {
            sb.AppendLine(
                $"**{++i}.**\t<@{leaderboardEntry.InviterUser.Id}>\t**·**\t**{leaderboardEntry.Invites}** invites. (**{leaderboardEntry.PendingInvites}** pending - **{leaderboardEntry.PendingInvites + leaderboardEntry.Invites}** total)");
        }

        eb.Description = sb.ToString();
        return eb.Build();
    }
}