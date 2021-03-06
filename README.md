# Mod Info
EnhancedTwitchChat is a rich text Twitch chat integration mod with full unicode, emote, cheermote, and emoji support.

# Features
- Full Rich Text Support, see ALL of your Twitch chat when immersed in Beat Saber!
  - This includes all Twitch badges, emotes, cheermotes, BetterTwitchTV emotes, FrankerFaceZ emotes, all Emojis and even animated emotes!
  - This also includes full Unicode Support! This means you can enjoy the chat in any language!
- Customizable User Interface
  - Change the scale, width, text/background colors, and chat order!
- *Coming soon*
  - Popular alert API integration with services such as streamlabs
  - More chat customization options!

# Installation
After copying the contents of EnhancedTwitchChat.zip to your Beat Saber directory, run the game once to generate the EnhancedTwitchChat.ini file in the UserData folder inside your Beat Saber directory.

# Setup
All you need to enter is the channel name which you want to join, the chat will show up to your right in game, and you can move it by pointing at it with the laser from your controller and grabbing it with the trigger. You can also move the chat towards you and away from you by pressing up and down on the analog stick or trackpad on your controller. Finally, you can use the lock button in the corner of the chat to lock the chat position in place so you don't accidentally move it.

# Config
For the rest of the setup, you will have to manually edit the config file (in UserData\EnhancedTwitchChat.ini).  *Keep in mind all config options will update in realtime when you save the file! This means you don't have to restart the game to see your changes!* Use the table below as a guide for setting these values:

| Option                     | Description                                                                                                                  |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------|
| **TwitchChannel**          | The name of the Twitch channel whos chat you want to join                                                                    |
| **FontName**               | The name of the system font that should be used for chat messages. You can specify any font installed on your computer!      |
| **ChatScale**              | How large the chat messages/emotes should be displayed.                                                                      |
| **ChatWidth**              | The width of the chat, regardless of ChatScale.                                                                              |
| **LineSpacing**            | Determines the amount of extra spacing between lines of chat                                                                 |
| **MaxMessages**            | The maximum number of messages allowed in chat at once.                                                                      |
| **PositionX/Y/Z**          | The location of the chat in game (this can be adjusted in game, see description above!)                                      |
| **RotationX/Y/Z**          | The rotation of the chat in game (this can be adjusted in game, see description above!)                                      |
| **TextColorR/G/B/A**       | The color of chat messages, on a scale of 0-1. If your colors are between 0-255, just divide by 255 to get this value!       |
| **BackgroundColorR/G/B/A** | The color of the chat background, on a scale of 0-1. If your colors are between 0-255, just divide by 255 to get this value! |
| **LockChatPosition**       | Whether or not the chat can be moved by pointing at it with the controller laser and gripping with the trigger.              |
| **ReverseChatOrder**       | When set to true, chat messages will enter from the top and exit on bottom instead of entering on bottom and exiting on top. |

# Compiling
To compile this mod simply clone the repo and update the project references to reference the corresponding assemblies in the "Beat Saber\Beat Saber_Data\Managed" folder, then compile. You may need to remove the post build event if your Beat Saber directory isn't at the same location as mine.

# Download
[Click here to download the latest EnhancedTwitchChat.dll](https://github.com/brian91292/BeatSaber-EnhancedTwitchChat/releases/latest)
