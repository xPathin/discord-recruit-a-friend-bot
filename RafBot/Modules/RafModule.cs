// <copyright file="RafModule.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RafBot.Models;
using RafBot.Persistence;

namespace RafBot.Modules;

/// <summary>
/// Recruit a friend module.
/// </summary>
[Group("raf")]
public class RafModule : ModuleBase<SocketCommandContext>
{
    private readonly AppDbContext _dbContext;
    private RafDataManager? _rafSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RafModule"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public RafModule(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// !raf setup command handler.
    /// Setups the event.
    /// </summary>
    /// <param name="guildChannel">The channel to invite users to.</param>
    /// <param name="rafVerifiedRole">The role a user needs to count as verified.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    [Command("setup")]
    public async Task RafSetupAsync(IGuildChannel guildChannel, IRole? rafVerifiedRole = null)
    {
        var sb = new StringBuilder();
        var eb = new EmbedBuilder()
        {
            Title = "RAF-Bot Setup",
            Color = Color.Green,
        };

        if (_rafSettings!.RafSetup)
        {
            eb.Description = "Event is already setup, please type ``!raf reset`` if you want to start over.";
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            await ReplyAsync(embed: eb.Build());
            return;
        }

        if (guildChannel.GetType() != typeof(SocketTextChannel))
        {
            sb.AppendLine($"{guildChannel.Name} is not a text channel, please select a text channel!");
            sb.AppendLine($"Setup failed, please try again!");
            eb.Description = sb.ToString();
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            await ReplyAsync(embed: eb.Build());
            return;
        }

        if (rafVerifiedRole != null)
        {
            _rafSettings.RafVerifiedRoleId = rafVerifiedRole.Id;
        }

        _rafSettings.RafChannelId = guildChannel.Id;
        _rafSettings.RafSetup = true;
        sb.AppendLine($"Invite Channel set to: <#{guildChannel.Name}>");
        if (_rafSettings.RafVerifiedRoleId != null)
        {
            sb.AppendLine($"Verified Role set to: <@{rafVerifiedRole?.Name}>");
        }

        sb.AppendLine($"Setup successful, you can start the event now by typing ``!raf start``");
        eb.Description = sb.ToString();
        eb.Title = $"✅ {eb.Title}";
        await ReplyAsync(embed: eb.Build());
    }

    /// <summary>
    /// !raf reset command.
    /// Resets the event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    [Command("reset")]
    public async Task RafResetAsync()
    {
        var eb = new EmbedBuilder()
        {
            Title = "✅ RAF-Bot Reset",
            Description = "All  settings and invite codes have been reset.\r\nplease setup again by typing:\r\n``!raf setup #invite-channel optional: @invite-role``.",
            Color = Color.Green,
        };

        await _rafSettings!.ResetAsync(Context.Client.CurrentUser.Id);
        await ReplyAsync(embed: eb.Build());
    }

    /// <summary>
    /// !raf start command.
    /// Starts the event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    [Command("start")]
    public async Task RafStartAsync()
    {
        var eb = new EmbedBuilder()
        {
            Title = "RAF-Bot",
            Color = Color.Green,
        };
        if (!_rafSettings!.RafSetup)
        {
            await ReplyEventNotSetup();
            return;
        }

        if (!_rafSettings.RafActive)
        {
            _rafSettings.RafActive = true;
        }
        else
        {
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            eb.Description = "There is already a event running, please type ``!raf stop.`` to stop the event.";
            await ReplyAsync(embed: eb.Build());
            return;
        }

        eb.Title = $"✅ {eb.Title}";
        eb.Description = "RAF event was started! Stop any time by typing ``!raf stop``. Users can now join the event and get an invite URL by typing: ``!raf join``.";
        await ReplyAsync(embed: eb.Build());
    }

    /// <summary>
    /// !raf stop command.
    /// Stops the event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    [Command("stop")]
    public async Task RafStopAsync()
    {
        var eb = new EmbedBuilder()
        {
            Title = "RAF-Bot",
            Color = Color.Green,
        };

        if (!_rafSettings!.RafSetup)
        {
            await ReplyEventNotSetup();
            return;
        }

        if (_rafSettings.RafActive)
        {
            _rafSettings.RafActive = false;
        }
        else
        {
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            eb.Description = "There is no event running, please type ``!raf start.`` to start the event.";
            await ReplyAsync(embed: eb.Build());
            return;
        }

        eb.Title = $"✅ {eb.Title}";
        eb.Description = "Event was stopped, please type ``!raf results`` to see the event results. You can can type ``!raf start`` any time to resume the event.";
        await ReplyAsync(embed: eb.Build());
    }

    /// <summary>
    /// !raf join command.
    /// User joins the event.
    /// </summary>
    /// <exception cref="ApplicationException">Throws when there was an error.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [Command("join")]
    public async Task RafJoin()
    {
        var eb = new EmbedBuilder()
        {
            Title = "RAF-Bot",
            Color = Color.Green,
        };

        if (!_rafSettings!.RafActive)
        {
            eb.Color = Color.LightOrange;
            eb.Title = $"❗ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: There is no event running right now. 😔";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
            return;
        }

        if (await _rafSettings!.UserHasInviteAsync(Context.User.Id))
        {
            eb.Color = Color.LightOrange;
            eb.Title = $"❗ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: You have already joined this event, please type ``!raf link`` to get your invitation link.";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
            return;
        }

        var inviteChannel = Context.Guild.GetTextChannel(_rafSettings.RafChannelId);
        if (inviteChannel == null)
        {
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: The invite channel was not found, it looks like the channel was deleted. Please let the Staff know! 🙏";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
            return;
        }

        var inviteCode = await inviteChannel.CreateInviteAsync(maxAge: 0, isUnique: true);
        if (await _rafSettings.UserAddInvite(inviteCode, Context.User.Id))
        {
            var userInvite = await _rafSettings.UserGetInviteCode(Context.User.Id) ?? throw new ApplicationException();
            eb.Title = $"✅ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: Thank you for joining the event, your invite code is: {userInvite.Code}.\r\nInvite other users by sending them the following URL: {userInvite.InviteUrl}";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
        }
        else
        {
            eb.Color = Color.Red;
            eb.Title = $"❌ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: Error creating the invite code. Please report this to the Staff.";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
        }
    }

    /// <summary>
    /// !raf link command.
    /// Displays a user's invite link.
    /// </summary>
    /// <exception cref="ApplicationException">Throws when there was an error.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [Command("link")]
    public async Task RafLink()
    {
        var eb = new EmbedBuilder()
        {
            Color = Color.Green,
            Title = "RAF-Bot",
        };

        if (!_rafSettings!.RafActive)
        {
            eb.Color = Color.LightOrange;
            eb.Title = $"❗ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: There is no event running right now. 😔";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
            return;
        }

        if (!await _rafSettings!.UserHasInviteAsync(Context.User.Id))
        {
            eb.Color = Color.LightOrange;
            eb.Title = $"❗ {eb.Title}";
            eb.Description = $"<@{Context.User.Id}>: You have not joined the event yet, please type: ``!raf join`` to join.";
            await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
            return;
        }

        var inviteCode = await _rafSettings.UserGetInviteCode(Context.User.Id) ?? throw new ApplicationException();
        eb.Title = $"✅ {eb.Title}";
        eb.Description = $"<@{Context.User.Id}>: Your invite code is: {inviteCode.Code}.\r\nInvite other Users, by using the following URL: {inviteCode.InviteUrl}";
        await ReplyAsync(embed: eb.Build(), messageReference: Context.Message.Reference);
    }

    /// <summary>
    /// !raf leaderboard / !raf results command.
    /// Displays the invitation leaderboard.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RequireContext(ContextType.Guild)]
    [Command("leaderboard")]
    [Alias("results")]
    public async Task RafLeaderboardAsync()
    {
        var rl = new RafLeaderboard();

        foreach (var inviteCode in _dbContext.InviteCodes)
        {
            var inviter = Context.Guild.GetUser(inviteCode.UserId);
            if (inviter == null)
            {
                continue;
            }

            var userInvites = await _dbContext.UserInvites.Where(x => x.InviteCodeId.Equals(inviteCode.Code))
                .ToListAsync();
            var leaderboardEntry = new RafLeaderboard.RafLeaderboardEntry(inviter);
            foreach (var userInvite in userInvites)
            {
                var invitedUser = Context.Guild.GetUser(userInvite.InvitedUserId);
                if (invitedUser == null)
                {
                    continue;
                }

                var pending = false;
                if (DateTime.Now - userInvite.JoinDate < new TimeSpan(0, 0, 15, 0))
                {
                    pending = true;
                }
                else if (_rafSettings!.RafVerifiedRoleId != null)
                {
                    var verifiedRole = Context.Guild.GetRole(_rafSettings!.RafVerifiedRoleId.Value);
                    pending = !invitedUser.Roles.Contains(verifiedRole);
                }

                if (pending)
                {
                    leaderboardEntry.PendingInvites++;
                }
                else
                {
                    leaderboardEntry.Invites++;
                }
            }

            rl.LeaderboardEntries.Add(leaderboardEntry);
        }

        await ReplyAsync(embed: rl.PrintLeaderboard());
    }

    /// <inheritdoc/>
    protected override void BeforeExecute(CommandInfo command)
    {
        _rafSettings = new RafDataManager(Context.Guild, _dbContext);
        base.BeforeExecute(command);
    }

    private async Task ReplyEventNotSetup()
    {
        await ReplyAsync(
            "Event is not setup, please setup by typing ``!raf setup #invite-channel optional: @invite-role``. ✅.",
            messageReference: Context.Message.Reference);
    }
}