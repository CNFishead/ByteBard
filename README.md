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

### `/economy daily`
> Claim your daily reward.
- Tied to the configured daily currency for the server
- Tracks streaks and gives randomized rewards
- Reward amount scales with streak length

---

### `/economy modifycurrency <@user> <currency> <amount>`
> Admin command to adjust a user's balance
- Positive values add
- Negative values subtract (but cannot drop below zero)
- Balance is updated only for the current server

---

### `/economy listcurrencies`
> View all currencies created for this server
- Shows currencies in an embed layout
- Only shows currencies tied to the server the command is used in

---

### `/economy balance <currency>`
> Check your balance for a specific currency
- Only shows your balance for the current server
- Shows a value even if your balance is zero

---

### `/economy transfercurrency <@user> <currency> <amount>`
> Send currency to another user
- Sender must have enough funds
- Recipient receives a DM notification (if possible)
- Transactions are per-server and cannot be cross-server

---

### `/economy addcurrencytype <name>`
> Create a new currency for your server
- Currency is only visible and usable in the current server
- Prevents duplicate currency names within the same server

---

### `/economy setdailycurrency <currency>`
> Set which currency `/daily` rewards in this server
- Currency must already exist in the server
- Updates the server's `ServerSettings` record

---

## 🗂 File Structure Suggestion
```text
/Modules/Economy/
├── AddCurrencyTypeCommand.cs
├── BalanceCommand.cs
├── DailyCommand.cs
├── ListCurrenciesCommand.cs
├── ModifyCurrencyCommand.cs
├── SetDailyCurrencyCommand.cs
├── TransferCurrencyCommand.cs
└── BaseEconomyModule.cs
```

---

## ✅ Best Practices
- Always filter by `GuildId` when accessing user records, currencies, and balances
- Handle null records gracefully and create defaults when needed
- Prevent logic exploits (negative transfers, claiming in the wrong context, etc.)
- Consider adding cooldowns, logging, and audit history tables for production

---

Let me know if you'd like this README in `.md` file format or want help generating help messages from it in-bot! 🚀
