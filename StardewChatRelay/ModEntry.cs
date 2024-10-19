using Discord.WebSocket;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using static StardewValley.LocalizedContentManager;

namespace StardewChatRelay;

internal sealed class ModEntry : Mod
{
    private static ModConfig? _modConfig;
    private static DiscordClient? _discordClient;
    
    public override void Entry(IModHelper helper)
    {
        _modConfig = helper.ReadConfig<ModConfig>();
        ThrowIfBadConfig(_modConfig);
        
        _discordClient = new DiscordClient(_modConfig.BotToken, _modConfig.ChannelId);
        _discordClient.ConnectAsync();
        _discordClient.OnDiscordMessageReceived += HandleDiscordMessage;
        
        Harmony harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(HandleStardewMessage)))
        );

        helper.Events.GameLoop.SaveLoaded += OnServerStart;
        helper.Events.GameLoop.ReturnedToTitle += OnServerClose;
        helper.Events.GameLoop.DayStarted += OnDayStart;
        helper.Events.GameLoop.DayEnding += OnDayEnd;
        helper.Events.Multiplayer.PeerConnected += OnPlayerJoin;
        helper.Events.Multiplayer.PeerDisconnected += OnPlayerLeave;
    }
    
    private static void ThrowIfBadConfig(ModConfig config)
    {
        if (config.BotToken.Length == 0)
            throw new InvalidOperationException("Bot token field is empty.");
        if (config.ChannelId == 0)
            throw new InvalidOperationException("Channel Id field is empty.");
    }
    
    private static Farmer? GetPlayerById(long id)
    {
        if (id == Game1.player.UniqueMultiplayerID)
            return Game1.player;
        
        Game1.otherFarmers.TryGetValue(id, out Farmer farmer);
        return farmer;
    }

    private static void OnServerStart(object? _, SaveLoadedEventArgs args)
    {
        WorldDate now = WorldDate.Now();
        string format = _modConfig?.ServerStart ?? ModConfig.Default.ServerStart;
        SendDiscordMessage(format
            .Replace("%year%", now.Year.ToString())
            .Replace("%season%", now.Season.ToString())
            .Replace("%day%", now.DayOfMonth.ToString()));
    }

    private static void OnServerClose(object? _, ReturnedToTitleEventArgs args)
    {
        WorldDate now = WorldDate.Now();
        string format = _modConfig?.ServerClose ?? ModConfig.Default.ServerClose;
        SendDiscordMessage(format
            .Replace("%year%", now.Year.ToString())
            .Replace("%season%", now.Season.ToString())
            .Replace("%day%", now.DayOfMonth.ToString()));
    }

    private static void OnDayStart(object? _, DayStartedEventArgs args)
    {
        WorldDate now = WorldDate.Now();
        string format = _modConfig?.DayStart ?? ModConfig.Default.DayStart;
        SendDiscordMessage(format
            .Replace("%year%", now.Year.ToString())
            .Replace("%season%", now.Season.ToString())
            .Replace("%day%", now.DayOfMonth.ToString()));
    }

    private static void OnDayEnd(object? _, DayEndingEventArgs args)
    {
        WorldDate now = WorldDate.Now();
        string format = _modConfig?.DayEnd ?? ModConfig.Default.DayEnd;
        SendDiscordMessage(format
            .Replace("%year%", now.Year.ToString())
            .Replace("%season%", now.Season.ToString())
            .Replace("%day%", now.DayOfMonth.ToString()));
    }

    private static void OnPlayerJoin(object? _, PeerConnectedEventArgs args)
    {
        Farmer? farmer = GetPlayerById(args.Peer.PlayerID);
        if (farmer == null)
            return;
        
        string format = _modConfig?.PlayerJoinMessage ?? ModConfig.Default.PlayerJoinMessage;
        SendDiscordMessage(format
            .Replace("%player%", farmer.Name));
    }

    private static void OnPlayerLeave(object? _, PeerDisconnectedEventArgs args)
    {
        Farmer? farmer = GetPlayerById(args.Peer.PlayerID);
        if (farmer == null)
            return;
        
        string format = _modConfig?.PlayerLeaveMessage ?? ModConfig.Default.PlayerLeaveMessage;
        SendDiscordMessage(format
            .Replace("%player%", farmer.Name));
    }

    private static void HandleDiscordMessage(object? _, SocketMessage ctx)
    {
        string format = _modConfig?.StardewMessage ?? ModConfig.Default.StardewMessage;
        SendChatMessage(format
            .Replace("%user%", ctx.Author.Username)
            .Replace("%message%", ctx.Content)
            .Replace("%channel%", ctx.Channel.Name));
    }

    // ReSharper disable twice UnusedParameter.Local
    private static void HandleStardewMessage(long sourceFarmer, int chatKind, LanguageCode language, string message)
    {
        Farmer? farmer = GetPlayerById(sourceFarmer);
        if (farmer == null)
            return;
        
        // TODO: find better way to filter Discord relayed message
        // Check if the message is from discord.
        if (message.StartsWith("[Discord]"))
            return;
        
        string format = _modConfig?.DiscordMessage ?? ModConfig.Default.DiscordMessage;
        SendDiscordMessage(format
            .Replace("%player%", farmer.Name)
            .Replace("%message%", message));
    }

    private static async void SendDiscordMessage(string message)
    {
        if (_discordClient == null)
            return;
        
        await _discordClient.SendMessage(message);
    }
    
    private static void SendChatMessage(string message)
    {
        // Doesn't show up to everyone in the server
        // Game1.chatBox.addMessage(message, Color.Purple);
        
        // Instead send message as a player 
        Game1.chatBox.activate();
        Game1.chatBox.setText(message);
        Game1.chatBox.chatBox.RecieveCommandInput('\r');
    }
}