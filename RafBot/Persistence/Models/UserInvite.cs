// <copyright file="UserInvite.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System;

namespace RafBot.Persistence.Models;

/// <summary>
/// An invited user.
/// </summary>
public class UserInvite
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserInvite"/> class.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="invitedUserId">The invited user's ID.</param>
    /// <param name="inviteCodeId">The invite code ID.</param>
    public UserInvite(ulong guildId, ulong invitedUserId, string inviteCodeId)
    {
        GuildId = guildId;
        InvitedUserId = invitedUserId;
        InviteCodeId = inviteCodeId;
    }

    /// <summary>
    /// Gets or sets the invited user's guild.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Gets or sets the invited user's user ID.
    /// </summary>
    public ulong InvitedUserId { get; set; }

    /// <summary>
    /// Gets or sets the invited user's invite code.
    /// </summary>
    public InviteCode InviteCode { get; set; } = default!;

    /// <summary>
    /// Gets or sets the invited user's invite code ID.
    /// </summary>
    public string InviteCodeId { get; set; }

    /// <summary>
    /// Gets or sets the invited user's join date.
    /// </summary>
    public DateTime JoinDate { get; set; } = DateTime.Now;
}