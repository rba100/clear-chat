# Slack-like chat system

Back-end code is a bit messy, I'm tidying this.
Front-end code will be discarded in favour of some framework, probably React.

## TODO
- Reconnection logic
- Direct messages.
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
- .NET Core
- MS SQL Server
- AES256 message encryption (but it's basically just obfuscation since the key is all zeros).
- OWIN pipeline (maintainers should understand OWIN; it's very easy).
- Static HTML files.
- SignalR.
- Home-made auth based on HTTP basic auth + salted hashes in DB.

## Philosophy

- Participation requires acceptance that all communications are private in perpetuity.
- A place to express yourself when the modern world denies you your nature.