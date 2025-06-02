# 🔐 Command Restriction Guide for Admins

This guide explains how to restrict access to specific bot commands based on user roles in your Discord server.

> ⚠️ Only users with **Administrator permissions** or the **server owner** can modify command restrictions.

---

## 🧭 Overview

The bot allows you to lock specific commands (like `/modifycurrency`) to roles of your choice. This is managed through the `/admin` group of commands:

| Command                            | Description |
|------------------------------------|-------------|
| `/admin restrict-command`          | Restrict a specific command to a single role |
| `/admin unrestrict-command`        | Remove a role’s access to a specific command |
| `/admin list-restrictions`         | See which roles have access to which commands |

---

## 🚧 Default Behavior

By default, **commands are unrestricted**, meaning all users can run them. If a command is **not explicitly restricted**, it is considered **open to everyone**.

Use `/admin restrict-command` to lock down sensitive commands.

---

## 🛠️ Usage Examples

### ✅ Restricting a Command

Restrict `/modifycurrency` so only users with the "Admin" role can run it:

