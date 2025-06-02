# Welcome Message Formatting Guide

You can customize your **automated** and **manual welcome messages** using special placeholder tokens. These will be replaced with real user info when the message is sent.

---

## ✨ Supported Placeholders

| Placeholder       | Replaced With                                         |
| ----------------- | ----------------------------------------------------- |
| `{user}`          | Mention of the user being welcomed (e.g., `@NewUser`) |
| `{username}`      | Username of the user being welcomed                   |
| `{tag}`           | Full Discord tag of the user (e.g., `NewUser#1234`)   |
| `{requester}`     | Mention of the user who ran the `/welcome` command    |
| `{requestername}` | Username of the requester                             |
| `{requestertag}`  | Full Discord tag of the requester                     |

---

## 🔤 Special Characters

- To insert a **new line**, use `\\n`  
  Example:  
   `Welcome {user}!\nWe're so glad to have you here!`

---

## 🧪 Example Message

Input:
`📣 Message from {requester}:\nWelcome {user} to the server!\nPlease check out the #rules channel.`

Output (if requested by `@AdminUser` for `@NewUser`):

```
📣 Message from @AdminUser:
Welcome @NewUser to the server!
Please check out the #rules channel.
```

---

## 🛠 Best Practices

- Keep the message friendly, short, and clear.
- Use `\\n` to break up longer blocks into paragraphs.
- Use `{user}` and `{requester}` to make the message feel more personal.
- Avoid ping spam or long inline formatting (no Discord markdown like `**bold**` or `__underline__` unless intended).

---

## 📌 Notes

- If `{requester}` is used in the **automated welcome message**, it will default to `System`.
- If a placeholder is not recognized, it will be left unchanged in the output.
