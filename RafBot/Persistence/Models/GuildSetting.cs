// <copyright file="GuildSetting.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

namespace RafBot.Persistence.Models;

/// <summary>
/// Event setting of a discord guild.
/// </summary>
public class GuildSetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuildSetting"/> class.
    /// </summary>
    /// <param name="guildId">The guild id.</param>
    /// <param name="key">The settings key.</param>
    /// <param name="value">The settings value.</param>
    public GuildSetting(ulong guildId, string key, string? value)
    {
        GuildId = guildId;
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Gets or sets the guild id.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Gets or sets the settings key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the settings value.
    /// </summary>
    public string? Value { get; set; }
}