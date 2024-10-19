StardewChatRelay
================

A Stardew Valley <-> Discord bridge for relaying messages between each other.

Features
--------

- Relays message between Stardew Valley and Discord.
- Shows server startup and closing, player joins, and day change messages.

Setup
-----

Start Stardew Valley and close to generate the config file.

Find the `config.json`, put the bot token and channel id into the configs, and save.

```json
{
  "BotToken": "BOT_TOKEN_HERE",
  "ChannelId": CHANNEL_ID_HERE,
  "ServerStart": ":green_circle: **Server has started.** Currently on Year %year% %season% %day%.",
  ...
}
```

TODO
----

- Add Event messages
- Add playing status to the bot.
- /info to get the world info.
- /list to list the players currently online.
- /ip to get public ip?