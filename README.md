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
- Reconnection logic
- Direct messages.
- Be in multiple channels or DMs at once.
- Editable and deletable messages.
- Emoji support.
- Choose UI framework(s). Currently Bootstrap + jQuery.
  - UI component for login.
  - UI component to manage current identity. I.e. you can be have more than one identity 'logged in' and you select one you wish to associate with your next message.
  - UI component for messages.
  - UI component for user name (in messages control).
  - UI component for text entry.
  - Auto-complete.
- Add favicon.
- Social justice warrior bot to ensure users repress any sentiment that could cause offence.
- Ability to upload images, initiated by pasting from the clipboard to the input bar.
- 'chap is typing...' system.
- more /slash commands for all the fun.
- server and SSL certificates

## Tech stack
As things are now. Update here as decisions are made.
- .NET Core
- MS SQL Server
- AES256 message encryption (but it's basically just obfuscation since the key is all zeros).
- OWIN pipeline (maintainers should understand OWIN; it's very easy).
- Static HTML files.
- SignalR.
- Home-made auth based on HTTP basic auth + salted hashes in DB.
