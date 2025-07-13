# Skill Check System Documentation

## Overview

The Skill Check System allows World Masters (WMs) to create skill checks that players can attempt in Discord channels. The system handles automatic validation of rolls against difficulty classes (DCs) and provides custom success/failure messages.

## Commands

### `/skill init-skillcheck`

Creates a new skill check for players to attempt.

**Parameters:**

- `skill` (required): The name of the skill (e.g., "Perception", "Athletics", "Stealth")
- `dc` (required): The Difficulty Class (DC) that players must meet or exceed
- `successMessage` (optional): Custom message shown when players succeed
- `failureMessage` (optional): Custom message shown when players fail
- `durationMinutes` (optional): How long the skill check remains active (auto-expires)
- `isPrivate` (optional): If true, results are only sent via DM (default: false)

**Example:**

```
/skill init-skillcheck skill:Perception dc:15 successMessage:"You notice something hidden!" failureMessage:"You see nothing unusual." durationMinutes:60 isPrivate:true
```

**Privacy Modes:**

- **Public (default)**: Success/failure shown in channel, detailed messages sent via DM
- **Private**: All results sent only via DM, channel only shows that an attempt was made

### `/skill help`

Get comprehensive help and documentation for skill check commands.

**Example:**

```
/skill help
```

### `/skill help-privacy`

Learn about privacy modes and information security features.

**Example:**

```
/skill help-privacy
```

### `/skill help-examples`

See practical examples of skill check usage scenarios.

**Example:**

```
/skill help-examples
```

### `/skill roll`

Allows players to attempt a skill check with their roll result.

**Parameters:**

- `skill` (required): The name of the skill check to attempt
- `rollResult` (required): The player's dice roll result

**Example:**

```
/skill roll skill:Perception rollResult:18
```

**Notes:**

- Each player can only attempt each skill check once
- Results are displayed publicly so other players can see outcomes
- Success messages are also sent privately via DM when possible

### `/skill list-skillchecks`

Lists all active skill checks in the current channel.

**Example:**

```
/skill list-skillchecks
```

### `/skill status`

Shows detailed status of a specific skill check.

**Parameters:**

- `skill` (required): The name of the skill check to view

**Example:**

```
/skill status skill:Perception
```

### `/skill remove-skillcheck`

Removes/deactivates a skill check.

**Parameters:**

- `skill` (required): The name of the skill check to remove

**Example:**

```
/skill remove-skillcheck skill:Perception
```

## Features

### Automatic Cleanup

- Skill checks with expiration times are automatically cleaned up when they expire
- The cleanup service runs every hour to remove expired skill checks
- Expired skill checks are marked as inactive rather than deleted (preserves history)

### Player Restrictions

- Each player can only attempt each skill check once
- Duplicate attempts are prevented and the player is notified of their previous attempt

### Guild and Channel Specific

- Skill checks are specific to both the Discord server (guild) and channel
- Different channels can have different skill checks running simultaneously
- Skill check names must be unique within a channel (case-insensitive)

### Rich Embeds

- All responses use Discord embeds for better formatting
- Color coding: Blue for info, Green for success, Red for failure, Orange for warnings
- Timestamps and user attribution included

### Database Persistence

- All skill checks and attempts are stored in the database
- Maintains history of all attempts for statistics and reference
- Supports data analysis and reporting

### Information Security

- **DCs are never shown publicly** - Only visible to the skill check creator
- **Success/failure messages are sent privately via DM** to maintain secrecy
- **Private skill checks** send all results via DM only, keeping everything confidential
- **Status command** shows different information based on whether you created the skill check
- **List command** doesn't reveal DCs or messages, only basic status information

## Database Schema

### SkillCheck Table

- `Id`: Primary key
- `GuildId`: Discord server ID
- `ChannelId`: Discord channel ID
- `SkillName`: Name of the skill
- `DC`: Difficulty Class
- `SuccessMessage`: Custom success message
- `FailureMessage`: Custom failure message
- `CreatedByUserId`: Discord user ID of creator
- `CreatedAt`: When the skill check was created
- `ExpiresAt`: When the skill check expires (nullable)
- `IsActive`: Whether the skill check is currently active

### SkillCheckAttempt Table

- `Id`: Primary key
- `SkillCheckId`: Foreign key to SkillCheck
- `UserId`: Discord user ID of the player
- `RollResult`: The player's roll result
- `Success`: Whether the attempt succeeded
- `AttemptedAt`: When the attempt was made

## Usage Examples

### Basic Skill Check

A WM wants to set up a simple perception check:

```
/skill init-skillcheck skill:Perception dc:15
```

Players attempt it:

```
/skill roll skill:Perception rollResult:18  # Success!
/skill roll skill:Perception rollResult:12  # Failure
```

### Skill Check with Custom Messages

A WM wants to create an investigation check with specific outcomes:

```
/skill init-skillcheck skill:Investigation dc:20 successMessage:"You discover a hidden compartment containing a valuable gem!" failureMessage:"The area appears ordinary."
```

### Private/Secret Skill Check

A WM wants to create a stealth check where only the player knows if they succeeded:

```
/skill init-skillcheck skill:Stealth dc:18 successMessage:"You remain undetected and overhear important information." failureMessage:"You accidentally make noise." isPrivate:true
```

### Temporary Skill Check

A WM wants a skill check that expires after 30 minutes:

```
/skill init-skillcheck skill:Stealth dc:18 durationMinutes:30
```

### Managing Multiple Skill Checks

View all active skill checks:

```
/skill list-skillchecks
```

Check status of a specific skill check:

```
/skill status skill:Perception
```

Remove a skill check:

```
/skill remove-skillcheck skill:Perception
```

## Error Handling

The system includes comprehensive error handling for:

- Invalid skill check names
- Duplicate skill checks in the same channel
- Expired skill checks
- Database connectivity issues
- Discord API failures
- User permission issues

All errors are logged and users receive appropriate feedback messages.

## Best Practices

1. **Naming**: Use clear, descriptive skill names that players will recognize
2. **DCs**: Set appropriate difficulty classes for your game system
3. **Messages**: Provide meaningful success/failure messages to enhance the gaming experience
4. **Expiration**: Use expiration times for temporary events or time-sensitive skill checks
5. **Cleanup**: Regularly remove completed skill checks to keep channels organized
