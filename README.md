# 🎵 ByteBard Discord Bot

**ByteBard** is a modular, extensible Discord bot designed to bring RPG flair, economy systems, interactive games, and more to any server it joins. Built using C# with Discord.Net and Entity Framework Core, ByteBard weaves code with character—bringing useful functionality wrapped in whimsical theming.

---

## 🗂️ Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Architecture](#architecture)
- [Commands](docs/commands.md)
- [Game Handlers](docs/game-handlers.md)
- [Economy System](docs/economy.md)
- [Database Schema](docs/database.md)
- [Server Settings](docs/server-settings.md)
- [Welcome and Join Roles](docs/welcome.md)
- [Deployment](docs/deployment.md)
- [Contributing](#contributing)
- [License](#license)

---

## ✨ Features

- Slash and prefix command support
- Extensible game system (casino-style games, etc.)
- Modular user economy with multiple currencies
- Server-specific configuration (roles, welcome messages)
- Dynamic status messages with thematic flavor
- Button interaction and replay handling

---

## 🚀 Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/yourusername/bytebard.git
   cd bytebard
   Set your environment variables:
   ```

2. Set Enviornment Variables
    ```env
    DISCORD_BOT_TOKEN – Your bot's token
    DATABASE_URL – Your PostgreSQL connection string
    ```

3. Run migrations and start the bot:
    ```bash
    dotnet ef database update
    dotnet run
    ```

## 🧠 Architecture
The core bot logic lives in BotService.cs and manages the lifecycle of Discord interactions.

Key components:
- Commands: Handled via CommandService for legacy prefix commands
- Interactions: Slash commands and buttons handled via InteractionService
- Database: BotDbContext.cs defines and configures all EF Core entities
- Handlers: Game-specific logic is injected via IGameHandlerRegistry


## 🤖 Commands
Slash and prefix commands are modularized for easy extension. Commands include:
- `/daily`
- `/balance`
- `/casino flip`
- `!ping`, etc.

📄 See full command reference in [Commands]('./Docs/Commands.md)

## 🎰 Game Handlers
Game logic is abstracted behind a registry interface, allowing you to plug in new games like slots, blackjack, etc., using the same interaction pipeline.

📄 Details in [Game Handlers](docs/game-handlers.md)

## 💰 Economy System
ByteBard supports:
- Multiple currency types
- Daily rewards
- User-specific balances
- Game-based wins/losses

📄 Read more in [Economy](docs/economy.md)

## 🛠️ Database Schema
Built with EF Core + PostgreSQL. Includes:
- UserRecord
- UserEconomy
- CurrencyType
- ServerSettings
- UserGameStats

📄 See [Database](docs/database.md)

## 🏰 Server Settings
Each guild can:
- Define default join roles
- Set welcome messages and channels
- Choose daily currency type (This allows some kind of daily interaction in the server)

📄 See [Server Settings](docs/server-settings.md)

## 🎉 Welcome System
New users can:
- Receive default roles automatically
- See a custom welcome message in a configured channel

📄 Setup instructions in [Welcome Documentation](./Docs/WelcomeDocumentation.md)

## 🛳️ Deployment
Supports local development and cloud-hosted deployment (e.g., Docker + Supabase PostgreSQL).

📄 See [Deployment](docs/deployment.md)

## 🤝 Contributing
Pull requests are welcome! If you're adding a new command, game, or feature:
- Create a new modular class in Modules/
- Follow the architecture and functional style
- Document your command in docs/commands.md
