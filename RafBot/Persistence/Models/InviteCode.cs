// <copyright file="InviteCode.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RafBot.Persistence.Models;

/// <summary>
/// A user's event invite code.
/// </summary>
public class InviteCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InviteCode"/> class.
    /// </summary>
    /// <param name="code">The invite code.</param>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="userId">The inviter user ID.</param>
    public InviteCode(string code, ulong guildId, ulong userId)
    {
        Code = code;
        GuildId = guildId;
        UserId = userId;
    }

    /// <summary>
    /// Gets or sets the invite code.
    /// </summary>
    [Key]
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the invite guild ID.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Gets or sets the inviter.
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// Gets or sets the number of uses on the invite code.
    /// </summary>
    public int Uses { get; set; } = 0;

    /// <summary>
    /// Gets the invite url.
    /// </summary>
    [NotMapped]
    public string InviteUrl => $"https://discord.gg/{Code}";
}