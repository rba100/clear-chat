# Slack-like chat system

## Philosophy

- Participation requires acceptance that all communications are private in perpetuity.
- A place to express yourself when the modern world denies you your nature.

## Architecture

### Backend
When a user sends a chat message, this will get processed by some handlers to see if it's actually a /slash command or just a chat message.
`ChatContext` is a POCO containing references to everything specific to the request, and this is passed to handlers as well so they
can get information about the user in question, the channel they're in, etc. Add stuff to `ChatContext` that relates to a single 'send message' from the UI.

### Frontend
Front-end code will be discarded in favour of some framework, probably React. Hack away, in the mean time.

## TODO
- JavaScript small tasks.
  - DateTime formatter (i.e. iso date to "5:54 PM").
  - Channel header information bar.
  - Make it work on Safari https://github.com/aspnet/SignalR-samples/blob/cd1e20844c47c5da5f51f74a5030c05a88152a8b/ChatSample/ChatSample/wwwroot/index.html#L75
- Reactions.
- Message popup-on-hover control (with buttons for, edit, delete, react to, etc).
- Direct messages.
- Editable messages.
- Auto-complete.
- Ability to upload images, initiated by pasting from the clipboard to the input bar.

## Tech stack
As things are now. Update here as decisions are made.
- .NET Core
- MS SQL Server
- AES256 message encryption (but it's basically just obfuscation since the key is all zeros).
- OWIN pipeline (maintainers should understand OWIN; it's very easy).
- Static HTML files.
- SignalR.
- Home-made auth based on HTTP basic auth + salted hashes in DB.
- Jism framework (HTML templating)
