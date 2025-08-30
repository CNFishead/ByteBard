# Advanced Dice Rolling Features

The Discord bot now supports advanced dice rolling features including PEMDAS mathematical operations and keep best/worst functionality.

## Getting Help

Use the `/dice-help` command in Discord to display an interactive, comprehensive guide with examples and explanations of all dice rolling features.

## Features

### 1. PEMDAS Mathematical Operations

The dice roller now supports proper order of operations (PEMDAS) for mathematical expressions:

**Examples:**

- `3d6x2+1` - Roll 3d6, multiply result by 2, then add 1
- `2d20+5x3-2` - Roll 2d20, add 5, multiply by 3, then subtract 2
- `1d10*2/4+3` - Roll 1d10, multiply by 2, divide by 4, then add 3

**Supported Operations:**

- `+` Addition
- `-` Subtraction
- `*` or `x` Multiplication
- `/` Division

### 2. Keep Best/Worst Rolls

You can now roll multiple dice and keep only the best or worst results:

**Keep Best Examples:**

- `4d6b3` - Roll 4d6, keep the best 3 rolls
- `6d8b4` - Roll 6d8, keep the best 4 rolls

**Keep Worst Examples:**

- `4d6w3` - Roll 4d6, keep the worst 3 rolls
- `5d10w2` - Roll 5d10, keep the worst 2 rolls

**Example Results:**

- Rolling `4d6b3` with rolls `[4, 6, 3, 1]` → keeps `[4, 6, 3]` → total: 13
- Rolling `4d6w3` with rolls `[4, 6, 3, 1]` → keeps `[4, 3, 1]` → total: 8

### 3. Combined Features

You can combine keep best/worst with mathematical operations:

**Examples:**

- `4d6b3x2+5` - Roll 4d6, keep best 3, multiply by 2, add 5
- `3d8w2+10` - Roll 3d8, keep worst 2, add 10

### 4. Traditional Features Still Supported

All existing features continue to work:

**Advantage/Disadvantage:**

- `1d20+3 adv` - Roll with advantage
- `1d20+3 disadv` - Roll with disadvantage

**Labels:**

- `1d20+3 "attack roll" adv` - Add descriptive labels
- `3d6x2+1 "damage"` - Label complex expressions

**Multiple Expressions:**

- `1d20+3 "attack"; 1d8+2 "damage"` - Multiple rolls separated by semicolons

## Available Commands

- `/roll [expression]` - Roll dice with the specified expression
- `/dice-help` - Display comprehensive help with examples and usage guide

## Usage Examples

```
/roll 3d6x2+1 "enhanced damage"
/roll 4d6b3 "character stats"
/roll 1d20+5 "attack" adv; 2d6+3x2 "crit damage"
/roll 6d8w4+10 "difficult challenge"
```

## Output Format

The bot will display:

- **Type**: Normal, ADV, DISADV, or ERROR
- **Label**: User-provided description (or "-" if none)
- **Expression**: The dice expression used
- **Rolls**: Individual die results (with kept rolls highlighted)
- **Total**: Final calculated result

For keep best/worst, the output shows both all rolled dice and which ones were kept.
