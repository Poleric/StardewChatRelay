namespace StardewChatRelay;

public sealed class ModConfig
{
    public static readonly ModConfig Default = new ModConfig(); 
    
    public String BotToken { get; set; } = "";
    public ulong ChannelId { get; set; } = 0;
    
    // Formats
    public string ServerStart { get; } = ":green_circle: **Server has started.**";
    public string ServerClose { get; } = ":red_circle: **Server has closed.**";
    public string DayStart { get; } = "**Year %year% %season% %day% has started.**";
    public string DayEnd { get; } = "**Everyone is sleeping.** Ending the day.";
    public string PlayerJoinMessage { get; } = ":inbox_tray: **%player%** has joined.";
    public string PlayerLeaveMessage { get; } = ":outbox_tray: **%player%** has left.";
    public string StardewMessage { get; } = "[Discord] %user%: %message%";
    public string DiscordMessage { get; } = "**<%player%>**: %message%";
}