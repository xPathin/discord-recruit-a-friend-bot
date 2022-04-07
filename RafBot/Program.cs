// <copyright file="Program.cs" company="palow">
// Copyright (c) palow. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RafBot.Persistence;
using RafBot.Persistence.Models;
using RafBot.Services;

namespace RafBot;

/// <summary>
/// Entry class.
/// </summary>
internal class Program
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private IServiceProvider _serviceProvider = default!;
    private DiscordSocketClient _client = default!;

    /// <summary>
    /// The program's main entrypoint.
    /// </summary>
    /// <param name="args">Command line args.</param>
    public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronous main method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task MainAsync()
    {
        var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
        if (string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("Bot token was not specified, please pass environment variable 'BOT_TOKEN'.");
            return;
        }

        // You should dispose a service provider created using ASP.NET
        // when you are finished using it, at the end of your app's lifetime.
        // If you use another dependency injection framework, you should inspect
        // its documentation for the best way to do this.
        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }

        await using var services = ConfigureServices();
        _serviceProvider = services;
        _client = services.GetRequiredService<DiscordSocketClient>();
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        _client.Log += LogAsync;
        services.GetRequiredService<CommandService>().Log += LogAsync;

        // Tokens should be considered secret data and never hard-coded.
        // We can read from the environment variable to avoid hard coding.
        // var token = "OTU5NTI0OTcxMTA3OTc1Mjk5.YkdJRA.cld65KAKZP3cIsAoArHDzoSl6sU";
        // await _client.LoginAsync(TokenType.Bot, token);
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        _client.Connected += OnClientConnectedAsync;
        _client.UserJoined += OnUserJoinedAsync;
        _client.UserLeft += OnUserLeftAsync;

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rafDataManager = new RafDataManager(user.Guild, dbContext);

            if (dbContext.UserInvites.Any(x => x.InvitedUserId.Equals(user.Id)) || !rafDataManager.RafActive)
            {
                await rafDataManager.UpdateInviteCodeUsesAsync();
                return;
            }

            // Only let new invites proceed and count when event is active
            var discordInviteCodes = (await user.Guild.GetInvitesAsync()).Where(x => x.Inviter.Id.Equals(_client.CurrentUser.Id)).ToList();
            var allDbInviteCodes = await dbContext.InviteCodes.ToListAsync();

            InviteCode? changedCode = null;
            foreach (var dbInviteCode in allDbInviteCodes)
            {
                var discordInvite = discordInviteCodes.FirstOrDefault(x => x.Code.Equals(dbInviteCode.Code));
                if (discordInvite == null)
                {
                    continue;
                }

                if (discordInvite.Uses > dbInviteCode.Uses)
                {
                    if (changedCode != null)
                    {
                        changedCode = null;
                        break;
                    }

                    changedCode = dbInviteCode;
                }
            }

            if (changedCode != null)
            {
                var userInvite = new UserInvite(user.Guild.Id, user.Id, changedCode.Code);
                dbContext.UserInvites.Add(userInvite);
                await dbContext.SaveChangesAsync();
            }

            await rafDataManager.UpdateInviteCodeUsesAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity =
                await dbContext.UserInvites.FirstOrDefaultAsync(x =>
                    x.GuildId.Equals(guild.Id) && x.InvitedUserId.Equals(user.Id));
            if (entity != null)
            {
                dbContext.Remove(entity);
            }

            await dbContext.SaveChangesAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task OnClientConnectedAsync()
    {
        await _semaphoreSlim.WaitAsync();
        await _client.DownloadUsersAsync(_client.Guilds);
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var allGuildIds = await dbContext.GuildSettings.Select(x => x.GuildId).Distinct().ToListAsync();
            foreach (var guildId in allGuildIds)
            {
                var guild = _client.GetGuild(guildId);

                if (guild == null)
                {
                    continue;
                }

                var rafDataManager = new RafDataManager(guild, dbContext);
                await rafDataManager.UpdateInviteCodeUsesAsync();
                await rafDataManager.UpdateUserInvitesAsync();
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(_ =>
            {
                var cfg = new DiscordSocketConfig();
                cfg.GatewayIntents |= GatewayIntents.GuildMembers;
                return new DiscordSocketClient(cfg);
            })
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<HttpClient>()
            .AddDbContext<AppDbContext>(x => x.UseSqlite($"Data Source=data/app.db;"))
            .BuildServiceProvider();
    }
}