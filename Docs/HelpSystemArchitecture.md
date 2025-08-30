# Help System Architecture

This document describes the centralized help system architecture for the FallVerseBotV2 Discord bot.

## Architecture Overview

The help system follows the same modular pattern as other bot features (like Casino), with clear separation of concerns:

```
Commands/Help/
├── BaseHelpModule.cs          # Base class for help modules
├── HelpModule.cs              # Main help group commands
└── Handlers/
    └── DiceHelpHandler.cs     # Business logic for dice help
```

## Structure Pattern

### 1. Base Module (`BaseHelpModule.cs`)

- Abstract base class for all help-related command modules
- Provides common dependencies (Logger, Database context)
- Ensures consistent structure across help commands

### 2. Group Module (`HelpModule.cs`)

- Defines the `/help` command group
- Contains slash command definitions
- Delegates business logic to appropriate handlers
- Follows dependency injection pattern

### 3. Handlers (`Handlers/`)

- Contains business logic for each help command
- Keeps modules lightweight and focused
- Enables easier testing and maintenance
- Follows single responsibility principle

## Command Structure

### Current Commands

- `/help dice` - Comprehensive dice rolling help
- `/help commands` - General commands help (placeholder)
- `/help economy` - Economy system help (placeholder)
- `/help admin` - Administrative commands help (placeholder)

### Adding New Help Commands

1. **Create Handler** (if complex logic needed):

   ```csharp
   // Commands/Help/Handlers/NewFeatureHelpHandler.cs
   public class NewFeatureHelpHandler
   {
       public async Task Run(SocketInteractionContext context) { /* logic */ }
   }
   ```

2. **Add to HelpModule**:

   ```csharp
   [SlashCommand("newfeature", "Help for new feature")]
   public async Task NewFeatureHelp() => await _newFeatureHandler.Run(Context);
   ```

3. **Register Handler** (in constructor):
   ```csharp
   _newFeatureHandler = new NewFeatureHelpHandler(_loggerFactory.CreateLogger<NewFeatureHelpHandler>(), db);
   ```

## Benefits of This Architecture

### 1. **Centralization**

- All help commands under `/help` group
- Easy discovery for users
- Consistent command structure

### 2. **Maintainability**

- Clear separation of concerns
- Business logic isolated in handlers
- Easy to add new help topics

### 3. **Consistency**

- Follows established patterns from Casino module
- Uses same dependency injection approach
- Consistent error handling and logging

### 4. **Scalability**

- Easy to add new help categories
- Handlers can be as simple or complex as needed
- Can reuse handlers across multiple commands if needed

## Migration Notes

### From Individual Commands

- Moved `/dice-help` → `/help dice`
- Old command marked as deprecated in documentation
- Business logic extracted to `DiceHelpHandler`

### Command Updates Required

- Update any documentation references
- Consider backwards compatibility if needed
- Update any hardcoded command references

## Future Enhancements

### Planned Help Topics

1. **Commands** (`/help commands`) - General bot commands overview
2. **Economy** (`/help economy`) - Currency and economy system
3. **Admin** (`/help admin`) - Administrative features
4. **Combat** (`/help combat`) - Combat tracking system
5. **Skill** (`/help skill`) - Skill check system

### Potential Features

- Dynamic help generation from command metadata
- Context-aware help (different help for different server settings)
- Interactive help with buttons/components
- Help search functionality
- Multi-language help support

## Example Usage

```
/help dice          # Comprehensive dice rolling guide
/help commands      # General bot commands (coming soon)
/help economy       # Economy system help (coming soon)
/help admin         # Administrative help (coming soon)
```

## Dependencies

- **Discord.NET**: For interaction handling and embeds
- **Microsoft.Extensions.Logging**: For structured logging
- **Entity Framework Core**: For database access (via BotDbContext)

## Error Handling

All help handlers include:

- Try-catch blocks for error handling
- Structured logging for debugging
- Ephemeral error responses to avoid channel clutter
- Graceful degradation for missing data
