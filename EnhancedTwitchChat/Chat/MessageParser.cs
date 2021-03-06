﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EnhancedTwitchChat.Utils;
using EnhancedTwitchChat.Chat;
using System.Text.RegularExpressions;
using EnhancedTwitchChat.UI;
using AsyncTwitch;
using static POCs.Sanjay.SharpSnippets.Drawing.ColorExtensions;
using Random = System.Random;


namespace EnhancedTwitchChat.Sprites
{
    public class SpriteInfo
    {
        public char swapChar;
        public string spriteIndex;
        public ImageType spriteType;
    }

    public class BadgeInfo : SpriteInfo
    {
    };

    public class EmoteInfo : SpriteInfo
    {
        public string swapString;
        public bool isEmoji;
    };

    public class MessageParser : MonoBehaviour
    {
        private static Dictionary<int, string> _userColors = new Dictionary<int, string>();
        public static void Parse(ChatMessage newChatMessage)
        {
            char swapChar = (char)0xE000;
            List<EmoteInfo> parsedEmotes = new List<EmoteInfo>();
            List<BadgeInfo> parsedBadges = new List<BadgeInfo>();

            bool isActionMessage = newChatMessage.msg.Substring(1).StartsWith("ACTION") && newChatMessage.msg[0] == (char)0x1;
            if (isActionMessage)
                newChatMessage.msg = newChatMessage.msg.TrimEnd((char)0x1).Substring(8);

            // Parse and download any twitch emotes in the message
            if (newChatMessage.twitchMessage.Emotes.Count() > 0)
            {
                foreach (TwitchEmote e in newChatMessage.twitchMessage.Emotes)
                {
                    string emoteIndex = $"T{e.Id}";
                    if (!SpriteDownloader.CachedSprites.ContainsKey(emoteIndex))
                        SpriteDownloader.Instance.Queue(new SpriteDownloadInfo(emoteIndex, ImageType.Twitch, newChatMessage.twitchMessage.Id));
                    
                    int startReplace = Convert.ToInt32(e.Index[0][0]);
                    int endReplace = Convert.ToInt32(e.Index[0][1]);
                    string msg = newChatMessage.msg;

                    EmoteInfo swapInfo = new EmoteInfo();
                    swapInfo.swapChar = swapChar;
                    swapInfo.swapString = msg.Substring(startReplace, endReplace - startReplace + 1);
                    swapInfo.spriteIndex = emoteIndex;
                    swapInfo.spriteType = ImageType.Twitch;
                    parsedEmotes.Add(swapInfo);
                    swapChar++;
                }
                Thread.Sleep(5);
            }

            // Parse and download any twitch badges included in the message
            if (newChatMessage.twitchMessage.Author.Badges.Count() > 0)
            {
                foreach (Badge b in newChatMessage.twitchMessage.Author.Badges)
                {
                    string badgeName = $"{b.BadgeName}{b.BadgeVersion}";
                    string badgeIndex = string.Empty;
                    if (SpriteDownloader.TwitchBadgeIDs.ContainsKey(badgeName))
                    {
                        badgeIndex = SpriteDownloader.TwitchBadgeIDs[badgeName];
                        if (!SpriteDownloader.CachedSprites.ContainsKey(badgeIndex))
                            SpriteDownloader.Instance.Queue(new SpriteDownloadInfo(badgeIndex, ImageType.Badge, newChatMessage.twitchMessage.Id));

                        BadgeInfo swapInfo = new BadgeInfo();
                        swapInfo.swapChar = swapChar;
                        swapInfo.spriteIndex = badgeIndex;
                        swapInfo.spriteType = ImageType.Badge;
                        parsedBadges.Add(swapInfo);
                        swapChar++;
                    }
                }
                Thread.Sleep(5);
            }

            // Parse and download any emojis included in the message
            var matches = Utilities.GetEmojisInString(newChatMessage.msg);
            if (matches.Count > 0)
            {
                List<string> foundEmojis = new List<string>();
                foreach (Match m in matches)
                {
                    string emojiIndex = Utilities.WebParseEmojiRegExMatchEvaluator(m);
                    string replaceString = m.Value;

                    if (emojiIndex != String.Empty)
                    {
                        emojiIndex += ".png";
                        if (!SpriteDownloader.CachedSprites.ContainsKey(emojiIndex))
                            SpriteDownloader.Instance.Queue(new SpriteDownloadInfo(emojiIndex, ImageType.Emoji, newChatMessage.twitchMessage.Id));

                        if (!foundEmojis.Contains(emojiIndex))
                        {
                            foundEmojis.Add(emojiIndex);
                            EmoteInfo swapInfo = new EmoteInfo();
                            swapInfo.spriteType = ImageType.Emoji;
                            swapInfo.isEmoji = true;
                            swapInfo.swapChar = swapChar;
                            swapInfo.swapString = replaceString;
                            swapInfo.spriteIndex = emojiIndex;
                            parsedEmotes.Add(swapInfo);
                            swapChar++;
                        }
                    }
                }
                parsedEmotes = parsedEmotes.OrderByDescending(o => o.swapString.Length).ToList();
                Thread.Sleep(5);
            }

            // Parse and download any BTTV/FFZ emotes and cheeremotes in the message
            string[] msgParts = newChatMessage.msg.Split(' ').Distinct().ToArray();
            foreach (string w in msgParts)
            {
                string word = w;
                //Plugin.Log($"WORD: {word}");
                string spriteIndex = String.Empty;
                ImageType spriteType = ImageType.None;
                if (SpriteDownloader.BTTVEmoteIDs.ContainsKey(word))
                {
                    spriteIndex = $"B{SpriteDownloader.BTTVEmoteIDs[word]}";
                    spriteType = ImageType.BTTV;
                }
                else if (SpriteDownloader.BTTVAnimatedEmoteIDs.ContainsKey(word))
                {
                    spriteIndex = $"AB{SpriteDownloader.BTTVAnimatedEmoteIDs[word]}";
                    spriteType = ImageType.BTTV_Animated;
                }
                else if (SpriteDownloader.FFZEmoteIDs.ContainsKey(word))
                {
                    spriteIndex = $"F{SpriteDownloader.FFZEmoteIDs[word]}";
                    spriteType = ImageType.FFZ;
                }
                else if (newChatMessage.twitchMessage.GaveBits && Utilities.cheermoteRegex.IsMatch(word.ToLower()))
                {
                    Match match = Utilities.cheermoteRegex.Match(word.ToLower());
                    string prefix = match.Groups["Prefix"].Value;
                    if (SpriteDownloader.TwitchCheermoteIDs.ContainsKey(prefix))
                    {
                        int bits = Convert.ToInt32(match.Groups["Value"].Value);
                        string tier = SpriteDownloader.TwitchCheermoteIDs[prefix].GetTier(bits);
                        spriteIndex = $"{prefix}{tier}";
                        spriteType = ImageType.Cheermote;
                    }
                }

                if (spriteType != ImageType.None)
                {
                    if (!SpriteDownloader.CachedSprites.ContainsKey(spriteIndex))
                        SpriteDownloader.Instance.Queue(new SpriteDownloadInfo(spriteIndex, spriteType, newChatMessage.twitchMessage.Id));

                    EmoteInfo swapInfo = new EmoteInfo();
                    swapInfo.spriteType = spriteType;
                    swapInfo.swapChar = swapChar;
                    swapInfo.swapString = word;
                    swapInfo.spriteIndex = spriteIndex;
                    parsedEmotes.Add(swapInfo);
                    swapChar++;
                }
            }
            Thread.Sleep(5);

            // Replace each emote with a unicode character from a private range; we'll draw the emote at the position of this character later on
            foreach (EmoteInfo e in parsedEmotes)
            {
                string extraInfo = String.Empty;
                if (e.spriteType == ImageType.Cheermote)
                {
                    Match cheermote = Utilities.cheermoteRegex.Match(e.swapString);
                    string numBits = cheermote.Groups["Value"].Value;
                    extraInfo = $"\u200A<color={SpriteDownloader.TwitchCheermoteIDs[cheermote.Groups["Prefix"].Value].GetColor(Convert.ToInt32(numBits))}><size=3><b>{numBits}</b></size></color>\u200A";
                }
                string replaceString = $"\u00A0{Drawing.spriteSpacing}{Char.ConvertFromUtf32(e.swapChar)}{extraInfo}";
                if (!e.isEmoji)
                {
                    string[] parts = newChatMessage.msg.Split(' ');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == e.swapString)
                            parts[i] = replaceString;
                    }
                    newChatMessage.msg = string.Join(" ", parts);
                }
                else
                {
                    // Replace emojis using the Replace function, since we don't care about spacing
                    newChatMessage.msg = newChatMessage.msg.Replace(e.swapString, replaceString);
                }
            }
            Thread.Sleep(5);

            //// TODO: Re-add tagging, why doesn't unity have highlighting in its default rich text markup?
            //// Highlight messages that we've been tagged in
            //if (Plugin._twitchUsername != String.Empty && msg.Contains(Plugin._twitchUsername)) {
            //    msg = $"<mark=#ffff0050>{msg}</mark>";
            //}

            string displayColor = newChatMessage.twitchMessage.Author.Color;
            if ((displayColor == null || displayColor == String.Empty) && !_userColors.ContainsKey(newChatMessage.twitchMessage.Author.DisplayName.GetHashCode()))
            {
                // Generate a random color
                Random rand = new Random(newChatMessage.twitchMessage.Author.DisplayName.GetHashCode());
                int r = rand.Next(255);
                int g = rand.Next(255);
                int b = rand.Next(255);

                // Convert it to a pastel color
                System.Drawing.Color pastelColor = Drawing.GetPastelShade(System.Drawing.Color.FromArgb(255, r, g, b));
                int argb = ((int)pastelColor.R << 16) + ((int)pastelColor.G << 8) + (int)pastelColor.B;
                string colorString = String.Format("#{0:X6}", argb) + "FF";
                _userColors.Add(newChatMessage.twitchMessage.Author.DisplayName.GetHashCode(), colorString);
            }
            newChatMessage.twitchMessage.Author.Color = (displayColor == null || displayColor == string.Empty) ? _userColors[newChatMessage.twitchMessage.Author.DisplayName.GetHashCode()] : displayColor;

            // Add the users name to the message with the correct color
            newChatMessage.msg = $"<color={newChatMessage.twitchMessage.Author.Color}><b>{newChatMessage.twitchMessage.Author.DisplayName}</b></color><color=#00000000>|</color>{newChatMessage.msg}";

            // Prepend the users badges to the front of the message
            string badgeStr = String.Empty;
            if (parsedBadges.Count > 0)
            {
                parsedBadges.Reverse();
                for (int i = 0; i < parsedBadges.Count; i++)
                    badgeStr = $" {Drawing.spriteSpacing}{Char.ConvertFromUtf32(parsedBadges[i].swapChar)}\u2004{badgeStr}";
            }
            newChatMessage.msg = $"{badgeStr}\u2004{newChatMessage.msg}";

            // Italicize action messages and make the whole message the color of the users name
            if (isActionMessage)
                newChatMessage.msg = $"<i><color={newChatMessage.twitchMessage.Author.Color}>{newChatMessage.msg}</color></i>";

            // Finally, store our parsedEmotes and parsedBadges lists and render the message
            newChatMessage.parsedEmotes = parsedEmotes;
            newChatMessage.parsedBadges = parsedBadges;
            TwitchIRCClient.RenderQueue.Push(newChatMessage);
        }
    };
}
