# WaterBot
The Discord bot that prevents you from dehydrating!

## About
Ever felt dehydrated after forgetting to drink water while spending long hours at your computer?
This Discord bot solves this exact problem!

It allows you to set your wake time, sleep time, target amount of water per day and the amount
of water you should drink per notification.

It will then notify you every N amount of time reminding you to drink water and how much.

Follow this link to [invite the bot](https://discord.com/api/oauth2/authorize?client_id=812090297643302942&permissions=10240&scope=bot) to your own server.

## Development
If you want to get to work on this bot, follow the steps below. But first, make sure your system
has [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet/3.1) installed.

Now, go ahead and clone the repository.
```
git clone https://github.com/DelightedCat/WaterBot.git
cd WaterBot
git checkout develop
```
Copy the `config.example.json` file and name it `config.json`:
```
cp config.example.json config.json
```
Open the `config.json` with your favorite text editor and fill in the following fields:

- `Token` should be the unique (secret) token for your [application](https://discord.com/developers/applications).
- `DataDir` is the directory where all your data is saved, e.g. `/var/waterbot/data`.

In order to configure custom emojis, set `UseCustomEmojis` to true. Add the emojis from the `assets` directory to your server.

Fetch the right values for the emojis using the following example.

![Emojis](https://camo.githubusercontent.com/969866b03b1db389245aa44dd3cda12943ab8c5786bfd54f5245ee8f1cbe1e9a/68747470733a2f2f692e696d6775722e636f6d2f3974593733364e2e676966)

To enable submitting your members' current streak to the global leaderboard, set the option `EnableLeaderboard` to true.
Keep in mind that streaks also need to be enabled to make this possible.

Now to run the bot, use your IDE or type `dotnet run` in the console.

## Credits
This bot was largely done by [JustAeris](https://github.com/JustAeris). Check out his profile!
The library used to make this bot possible is [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).
