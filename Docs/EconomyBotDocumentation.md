# 💸 Economy Bot Commands - README

This document outlines the full set of modular, per-server scoped economy commands for your Discord bot.
Each command is separated into its own file, grouped under `economy`, and supports a unique in-server economy system.

---

## 🔧 Setup Summary

- Each command resides in a separate class inheriting from `BaseEconomyModule`
- Database access is injected through `BotDbContext`
- Each command is registered under the command group: `/economy`
- All commands are **guild-scoped**, meaning balances and currencies are isolated per server

---

## 📜 Command List

### `/daily`

> Claim your daily reward.

- Tied to the configured daily currency for the server
- Tracks streaks and gives randomized rewards
- Reward amount scales with streak length

---

### `/modifycurrency <@user> <currency> <amount>`

> Admin command to adjust a user's balance

- Positive values add
- Negative values subtract (but cannot drop below zero)
- Balance is updated only for the current server

---

### `/listcurrencies`

> View all currencies created for this server

- Shows currencies in an embed layout
- Only shows currencies tied to the server the command is used in

---

### `/balance <currency>`

> Check your balance for a specific currency

- Only shows your balance for the current server
- Shows a value even if your balance is zero

---

### `/transfercurrency <@user> <currency> <amount>`

> Send currency to another user

- Sender must have enough funds
- Recipient receives a DM notification (if possible)
- Transactions are per-server and cannot be cross-server

---

### `/addcurrencytype <name>`

> Create a new currency for your server

- Currency is only visible and usable in the current server
- Prevents duplicate currency names within the same server

---

### `/setdailycurrency <currency>`

> Set which currency `/daily` rewards in this server

- Currency must already exist in the server
- Updates the server's `ServerSettings` record

---

