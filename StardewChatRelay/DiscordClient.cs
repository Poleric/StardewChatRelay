using System.Diagnostics;
using Discord;
using Discord.WebSocket;

namespace StardewChatRelay;

public class DiscordClient
{
    private readonly string _botToken;
    private readonly ulong _channelId;
    private readonly DiscordSocketClient _socket;

    public event EventHandler<SocketMessage>? OnDiscordMessageReceived;
    
    public DiscordClient(string botToken, ulong channelId)
    {
        _botToken = botToken;
        _channelId = channelId;
        _socket = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged,
            LogLevel = LogSeverity.Verbose
        });

    }

    ~DiscordClient()
    {
        Disconnect();
    }
    
    public async void ConnectAsync()
    {
        await _socket.LoginAsync(TokenType.Bot, _botToken);
        await _socket.StartAsync();
        
        _socket.MessageReceived += DiscordMessageReceived;
        _socket.Connected += ConnectionSuccessful;
    }

    public async void Disconnect()
    {
        await _socket.StopAsync();
        
        _socket.MessageReceived -= DiscordMessageReceived;
        _socket.Connected -= ConnectionSuccessful;
    }

    private Task ConnectionSuccessful()
    {
        Debug.Print("Connected to discord.");
        Debug.Print($"Logged in as {_socket.CurrentUser.GlobalName}");
        return Task.CompletedTask;
    }
    
    private Task DiscordMessageReceived(SocketMessage msg)
    {
        // if msg is empty
        if (msg.Content is not { Length: > 1 }) return Task.CompletedTask;

        // if channel is not in config
        if (msg.Channel.Id != _channelId) return Task.CompletedTask;

        // if message is from bot
        if (msg.Author.IsBot) return Task.CompletedTask;

        OnDiscordMessageReceived?.Invoke(this, msg);
        return Task.CompletedTask;
    }

    public async Task SendMessage(string msg)
    {
        if (await _socket.GetChannelAsync(_channelId) is not IMessageChannel channel) return;
        
        await channel.SendMessageAsync(msg);
    }
    
}