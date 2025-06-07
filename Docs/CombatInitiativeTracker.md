# 🛡️ Combat Tracker Commands

The Combat Tracker system allows World Masters (WMs) and players to manage turn-based encounters in Discord. It supports dynamic combat order, round tracking, and flexible participant management.

---

## ⚔️ Setup & Turn Flow

### `/combat tracker-init <gameId>`
Creates a new tracker tied to the current channel and a custom `gameId`.

- Only one tracker per `gameId` per channel.
- Example: `/combat tracker-init goblin-ambush`

---

### `/combat tracker-add <gameId> <initiative:int> [name]`
Adds a combatant to the tracker with a specified initiative and optional name.

- If `name` is omitted, defaults to the user's Discord name.
- Players can add themselves or their companions.
- Combatants added during an active encounter will join at the start of the next round.

---

### `/combat tracker-start <gameId>`
Begins the tracker — sorts combatants by initiative and starts Round 1.

- The first user in initiative order will be mentioned and prompted to act.
- Only the WM (creator) or admins can start.

---

### `/combat tracker-next <gameId>`
Advances the tracker to the next turn.

- When all turns in the queue are used, the round is incremented and a new queue is built from the full combatant list.
- Mentions the next character’s controller.

---

## 🔄 Utility Commands

### `/combat tracker-setturn <gameId> <turnIndex:int>`
Sets the active turn to a specific index.

- Use this if the WM wants to manually skip or jump the queue.
- Index starts at `0`.
- Errors if index is out of bounds.
- Admin/WMs only.

---

### `/combat tracker-reset <gameId>`
Resets the tracker for a fresh start.

- Re-sorts initiative and starts over at Round 1.
- Useful for rerolling or restarting a battle without deleting it.

---

### `/combat tracker-delete <gameId>`
Deletes the tracker and its associated combatants.

- Must be called by the tracker creator or an administrator.
- This action cannot be undone.

---

### `/combat tracker-remove <gameId> <combatantName>`
Removes a combatant by name.

- Useful for removing defeated enemies or leaving players.
- Admin/WMs only.
- Also removes the combatant from the upcoming turn queue.

---

## 💡 Example Encounter Flow

```plaintext
1. /combat tracker-init goblin-raid
2. Players roll initiative and use /combat tracker-add
3. WM adds enemy NPCs using /combat tracker-add
4. WM uses /combat tracker-start goblin-raid
5. Each player uses /combat tracker-next when done with their turn
6. WM adds reinforcements mid-fight via /combat tracker-add
7. Tracker handles new round logic automatically
```

## 🧠 Notes
- Each channel can have multiple trackers (if they use unique gameIds).
- Combatants are tied to the user who added them (used for mention logic).
- Turn queue is round-based and regenerates each round.

### Tracker Expiration
- Each tracker stores a `LastUpdatedAt` timestamp.
- A background cleanup service removes trackers that haven't been updated for **7 days**.
