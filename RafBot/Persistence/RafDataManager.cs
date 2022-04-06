// <copyright file="RafDataManager.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RafBot.Persistence.Models;

namespace RafBot.Persistence;

/// <summary>
/// RAF-Event data manager.
/// </summary>
public class RafDataManager
{
    private const string _keyRafSetup = "RAF_SETUP";
    private const string _keyChannelId = "RAF_CHANNEL_ID";
    private const string _keyVerifiedRoleId = "RAF_VERIFIED_ROLE_ID";
    private const string _keyRafActive = "RAF_ACTIVE";

    private readonly IGuild _guild;
    private readonly AppDbContext _appDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RafDataManager"/> class.
    /// </summary>
    /// <param name="guild">The guild socket.</param>
    /// <param name="appDbContext">The application's database context.</param>
    public RafDataManager(SocketGuild guild, AppDbContext appDbContext)
    {
        _guild = guild;
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the event is setup.
    /// </summary>
    public bool RafSetup
    {
        get
        {
            var res = false;
            var guildSetting =
                _appDbContext.GuildSettings.FirstOrDefault(
                    x => x.Key.Equals(_keyRafSetup) && x.GuildId.Equals(_guild.Id));
            if (guildSetting != null)
            {
                bool.TryParse(guildSetting.Value, out res);
            }

            return res;
        }

        set
        {
            var setting =
                _appDbContext.GuildSettings.FirstOrDefault(
                    x => x.Key.Equals(_keyRafSetup) && x.GuildId.Equals(_guild.Id));
            if (setting == null)
            {
                setting = new GuildSetting(_guild.Id, _keyRafSetup, value.ToString());
                _appDbContext.Add(setting);
            }
            else
            {
                setting.Value = value.ToString();
            }

            _appDbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Gets or sets the invite channel ID.
    /// </summary>
    public ulong RafChannelId
    {
        get
        {
            ulong res = 0;
            var guildSetting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyChannelId) && x.GuildId.Equals(_guild.Id));
            if (guildSetting != null)
            {
                ulong.TryParse(guildSetting.Value, out res);
            }

            return res;
        }

        set
        {
            var setting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyChannelId) && x.GuildId.Equals(_guild.Id));
            if (setting == null)
            {
                setting = new GuildSetting(_guild.Id, _keyChannelId, value.ToString());
                _appDbContext.Add(setting);
            }
            else
            {
                setting.Value = value.ToString();
            }

            _appDbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Gets or sets event user verification role.
    /// </summary>
    public ulong? RafVerifiedRoleId
    {
        get
        {
            ulong? res = null;
            var guildSetting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyVerifiedRoleId) && x.GuildId.Equals(_guild.Id));
            if (guildSetting != null)
            {
                if (ulong.TryParse(guildSetting.Value, out var parseRes))
                {
                    res = parseRes;
                }
            }

            return res;
        }

        set
        {
            var setting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyVerifiedRoleId) && x.GuildId.Equals(_guild.Id));
            if (setting == null)
            {
                setting = new GuildSetting(_guild.Id, _keyVerifiedRoleId, value?.ToString());
                _appDbContext.Add(setting);
            }
            else
            {
                setting.Value = value?.ToString();
            }

            _appDbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the event is active.
    /// </summary>
    public bool RafActive
    {
        get
        {
            var res = false;
            var guildSetting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyRafActive) && x.GuildId.Equals(_guild.Id));
            if (guildSetting != null)
            {
                bool.TryParse(guildSetting.Value, out res);
            }

            return res;
        }

        set
        {
            var setting =
                _appDbContext.GuildSettings.FirstOrDefault(x =>
                    x.Key.Equals(_keyRafActive) && x.GuildId.Equals(_guild.Id));
            if (setting == null)
            {
                setting = new GuildSetting(_guild.Id, _keyRafActive, value.ToString());
                _appDbContext.Add(setting);
            }
            else
            {
                setting.Value = value.ToString();
            }

            _appDbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Add an invite code for a user to the data store.
    /// </summary>
    /// <param name="invite">The invite metadata.</param>
    /// <param name="userId">The inviters user ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> UserAddInvite(IInviteMetadata invite, ulong userId)
    {
        if (await UserHasInviteAsync(userId))
        {
            return false;
        }

        var inviteCode = new InviteCode(invite.Code, _guild.Id, userId);
        _appDbContext.InviteCodes.Add(inviteCode);
        await _appDbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks whether the user already has an invite code.
    /// </summary>
    /// <param name="userId">The inviters user ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> UserHasInviteAsync(ulong userId)
    {
        return Task.FromResult(_appDbContext.InviteCodes.Any(x => x.GuildId.Equals(_guild.Id) && x.UserId.Equals(userId)));
    }

    /// <summary>
    /// Gets a users invite code.
    /// </summary>
    /// <param name="userId">The inviters user ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<InviteCode?> UserGetInviteCode(ulong userId)
    {
        return await _appDbContext.InviteCodes.FirstOrDefaultAsync(x =>
            x.GuildId.Equals(_guild.Id) && x.UserId.Equals(userId));
    }

    /// <summary>
    /// Updates the uses of all invite codes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateInviteCodeUsesAsync()
    {
        var dbInviteCodes = await _appDbContext.InviteCodes.ToListAsync();
        var discordInviteCodes = await _guild.GetInvitesAsync();
        foreach (var dbInviteCode in dbInviteCodes)
        {
            var discordInvite = discordInviteCodes.FirstOrDefault(x => x.Code.Equals(dbInviteCode.Code));
            if (discordInvite == null)
            {
                continue;
            }

            if (discordInvite.Uses > 0 && discordInvite.Uses != dbInviteCode.Uses)
            {
                dbInviteCode.Uses = discordInvite.Uses.Value;
                await _appDbContext.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Updates the invite codes of all users.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateUserInvitesAsync()
    {
        var allInviteUsers = await _appDbContext.UserInvites.ToListAsync();
        foreach (var inviteUser in allInviteUsers)
        {
            if (await _guild.GetUserAsync(inviteUser.InvitedUserId) == null)
            {
                _appDbContext.Remove(inviteUser);
            }
        }

        await _appDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Resets the event completely.
    /// </summary>
    /// <param name="botId">The bots ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ResetAsync(ulong botId)
    {
        _appDbContext.RemoveRange(_appDbContext.GuildSettings.Where(x => x.GuildId.Equals(_guild.Id)));
        _appDbContext.RemoveRange(_appDbContext.InviteCodes.Where(x => x.GuildId.Equals(_guild.Id)));
        _appDbContext.RemoveRange(_appDbContext.UserInvites.Where(x => x.GuildId.Equals(_guild.Id)));

        var invites =
            (await _guild.GetInvitesAsync()).Where(x =>
                x.Inviter.Id.Equals(botId));
        foreach (var invite in invites)
        {
            await invite.DeleteAsync();
        }

        await _appDbContext.SaveChangesAsync();
    }
}