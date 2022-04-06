// <copyright file="RafLeaderboard.RafLeaderboardEntry.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using Discord;

namespace RafBot.Models;

/// <summary>
/// The recruit a friend leaderboard.
/// </summary>
public partial class RafLeaderboard
{
    /// <summary>
    /// A recruit a friend leaderboard entry.
    /// </summary>
    public class RafLeaderboardEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RafLeaderboardEntry"/> class.
        /// </summary>
        /// <param name="inviterUser">The inviter.</param>
        public RafLeaderboardEntry(IGuildUser inviterUser)
        {
            InviterUser = inviterUser;
        }

        /// <summary>
        /// Gets the inviter.
        /// </summary>
        public IGuildUser InviterUser { get; }

        /// <summary>
        /// Gets or sets the number of successful invites.
        /// </summary>
        public int Invites { get; set; }

        /// <summary>
        /// Gets or sets the number of pending invites.
        /// </summary>
        public int PendingInvites { get; set; }

        /// <summary>
        /// Gets the number of total invites.
        /// </summary>
        public int TotalInvites => PendingInvites + Invites;
    }
}