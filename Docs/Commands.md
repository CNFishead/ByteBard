## 🎲 Dice Commands

These commands provide advanced dice rolling capabilities with support for mathematical operations, advantage/disadvantage, and keep best/worst functionality.

---

### `/roll`

- **Description**: Roll dice with advanced features including PEMDAS operations, keep best/worst, and advantage/disadvantage.
- **Parameters**:
  - `message` (string): Dice expressions like `1d20+3 "attack" adv`, `4d6b3`, `3d6x2+1`
- **Features**:
  - **Basic Rolling**: `1d20+3`, `3d6-1`
  - **Advantage/Disadvantage**: `1d20+5 adv`, `1d20+5 disadv`
  - **PEMDAS Math**: `3d6x2+1`, `2d8+5*3-2` (supports `+`, `-`, `*`, `x`, `/`)
  - **Keep Best/Worst**: `4d6b3` (keep best 3), `4d6w3` (keep worst 3)
  - **Labels**: `1d20+3 "attack roll"`
  - **Multiple Expressions**: `1d20+3 "attack"; 1d8+2 "damage"`
- **Examples**:
  - `/roll 1d20+5 adv` - Roll attack with advantage
  - `/roll 4d6b3` - Roll stats (keep best 3 of 4)
  - `/roll 3d6x2+1 "enhanced damage"` - Complex math with label
  - `/roll 1d20+3 "attack"; 2d6+2 "damage"` - Multiple rolls

---

### `/dice-help`

- **Description**: Display comprehensive help with interactive examples and explanations of all dice rolling features.
- **Parameters**: None
- **Example**: `/dice-help`

---

## 🛡️ Admin Commands

These commands are grouped under `/admin` and are intended for server administrators to configure and manage bot behavior.

---

### `/admin add-join-role`

- **Description**: Assigns a role that new users will automatically receive upon joining the server.
- **Parameters**:
  - `role` (IRole): The role to assign.
- **Permissions**: Not Restricted
- **Example**: `/admin add-join-role role:@Newbie`

---

### `/admin remove-join-role`

- **Description**: Removes a role from the list of roles assigned to new users.
- **Parameters**:
- `role` (IRole): The role to remove.
- **Permissions**: Not Restricted
- **Example**: `/admin remove-join-role role:@Newbie`

---

### `/admin list-join-roles`

- **Description**: Lists all roles currently assigned to users when they join.
- **Parameters**: None
- **Permissions**: Not Restricted
- **Example**: `/admin list-join-roles`

---

### `/admin set-welcome-message`

- **Description**: Sets the welcome message template sent when users join the server.
- **Parameters**:
- `message` (string): The message content, supports dynamic tokens (like `{user}`).
- **Permissions**: Not Restricted
- **Example**: `/admin set-welcome-message message:"Welcome to the realm, {user}!"`

---

### `/admin set-welcome-channel`

- **Description**: Defines the channel where the welcome message will be posted.
- **Parameters**:
- `channel` (ITextChannel): The target channel.
- **Permissions**: Not Restricted
- **Example**: `/admin set-welcome-channel channel:#general`

---

### `/admin show-welcome-settings`

- **Description**: Displays the current welcome message and configured channel.
- **Parameters**: None
- **Permissions**: Not Restricted (By Default)
- **Example**: `/admin show-welcome-settings`

---

### `/admin set-manual-hello`

- **Description**: Updates the message used by the manual welcome command.
- **Parameters**:
- `message` (string): The static welcome text.
- **Permissions**: Not Restricted (By Default)
- **Example**: `/admin set-manual-hello message:"Greetings, traveler!"`

---

### `/admin restrict-command`

- **Description**: Restricts a specific slash command to a given role.
- **Parameters**:
- `commandName` (string): The command to restrict (case-insensitive).
- `role` (IRole): The role required to use the command.
- **Permissions**: Admin only
- **Example**: `/admin restrict-command commandName:"daily" role:@VIP`

---

### `/admin unrestrict-command`

- **Description**: Removes a role restriction from a previously restricted command.
- **Parameters**:
- `commandName` (string): The command to unrestrict.
- `role` (IRole): The role to remove from the restriction.
- **Permissions**: Admin only
- **Example**: `/admin unrestrict-command commandName:"daily" role:@VIP`

---

### `/admin list-restrictions`

- **Description**: Shows all commands currently restricted to specific roles.
- **Parameters**: None
- **Permissions**: Admin only
- **Example**: `/admin list-restrictions`
