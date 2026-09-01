using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("WelcomePanel", "WorldProject", "1.1.0")]
    [Description("Auto popup GUI Welcome Window upon first join / connect with Rules, Info, Store & Discord")]
    public class WelcomePanel : RustPlugin
    {
        private const string PanelMain = "WelcomePanel_Main";
        private HashSet<ulong> joinedPlayers = new HashSet<ulong>();

        private void Loaded()
        {
            joinedPlayers = Interface.Oxide.DataFileSystem.ReadObject<HashSet<ulong>>("WelcomePanel_Players") ?? new HashSet<ulong>();
        }

        private void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("WelcomePanel_Players", joinedPlayers);
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, PanelMain);
            }
        }

        private void Init()
        {
            cmd.AddChatCommand("info", this, nameof(CmdOpenMenu));
            cmd.AddChatCommand("help", this, nameof(CmdOpenMenu));
            cmd.AddChatCommand("rules", this, nameof(CmdOpenMenu));
            cmd.AddChatCommand("menu", this, nameof(CmdOpenMenu));
            cmd.AddChatCommand("discord", this, nameof(CmdOpenMenu));
        }

        // Автоматическое открытие при пробуждении (входе на сервер)
        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsConnected) return;

            // Небольшая задержка 1 секунда, чтобы клиент успел отрисовать мир
            timer.Once(1.0f, () =>
            {
                if (player != null && player.IsConnected)
                {
                    OpenPanel(player, "info");
                    
                    if (!joinedPlayers.Contains(player.userID))
                    {
                        joinedPlayers.Add(player.userID);
                        Interface.Oxide.DataFileSystem.WriteObject("WelcomePanel_Players", joinedPlayers);
                        SendReply(player, "<color=#55EFC4>[World Project]</color> Welcome to the server! Read the rules and enjoy playing.");
                    }
                }
            });
        }

        private void CmdOpenMenu(BasePlayer player, string command, string[] args)
        {
            string tab = args.Length > 0 ? args[0].ToLower() : "info";
            OpenPanel(player, tab);
        }

        [ConsoleCommand("welcomepanel.tab")]
        private void ConsoleSwitchTab(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            string tab = arg.GetString(0, "info");
            OpenPanel(player, tab);
        }

        [ConsoleCommand("welcomepanel.close")]
        private void ConsoleClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, PanelMain);
            SendReply(player, "<color=#55EFC4>[World Project]</color> Menu closed. You can reopen it anytime by typing <color=#FFEAA7>/info</color> or <color=#FFEAA7>/shop</color> in chat!");
        }

        private void OpenPanel(BasePlayer player, string tab)
        {
            CuiHelper.DestroyUi(player, PanelMain);

            var elements = new CuiElementContainer();

            // Главный фон с затемнением
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.06 0.07 0.09 0.97" },
                RectTransform = { AnchorMin = "0.14 0.12", AnchorMax = "0.86 0.88" },
                CursorEnabled = true
            }, "Overlay", PanelMain);

            // Верхняя плашка (Header)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.14 0.17 0.24 1.00" },
                RectTransform = { AnchorMin = "0 0.88", AnchorMax = "1 1" }
            }, PanelMain, "Header");

            elements.Add(new CuiLabel
            {
                Text = { Text = "<b>🔥 WELCOME TO WORLD PROJECT [EU] 2x TRIO</b>", FontSize = 19, Align = TextAnchor.MiddleLeft, Color = "0.95 0.95 0.95 1.0" },
                RectTransform = { AnchorMin = "0.03 0", AnchorMax = "0.75 1" }
            }, "Header");

            // Кнопка быстрого закрытия [✕]
            elements.Add(new CuiButton
            {
                Button = { Command = "welcomepanel.close", Color = "0.80 0.20 0.20 0.90" },
                RectTransform = { AnchorMin = "0.92 0.15", AnchorMax = "0.98 0.85" },
                Text = { Text = "<b>✕ CLOSE</b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "Header");

            // Боковое меню (Вкладки)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.09 0.11 0.15 1.00" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.26 0.88" }
            }, PanelMain, "LeftSidebar");

            AddTabButton(elements, "LeftSidebar", "📌 Server Rates & Info", "info", "0.78 0.96", tab == "info");
            AddTabButton(elements, "LeftSidebar", "📜 Rules & Anti-Cheat", "rules", "0.58 0.76", tab == "rules");
            AddTabButton(elements, "LeftSidebar", "🛒 VIP Kits & Store", "store", "0.38 0.56", tab == "store");
            AddTabButton(elements, "LeftSidebar", "💬 Discord Community", "discord", "0.18 0.36", tab == "discord");

            // Большая зеленая кнопка "AGREE & PLAY" внизу бокового меню
            elements.Add(new CuiButton
            {
                Button = { Command = "welcomepanel.close", Color = "0.15 0.70 0.35 1.00" },
                RectTransform = { AnchorMin = "0.05 0.03", AnchorMax = "0.95 0.14" },
                Text = { Text = "<b>✓ I AGREE & PLAY</b>", FontSize = 13, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "LeftSidebar");

            // Область контента
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.11 0.13 0.17 0.80" },
                RectTransform = { AnchorMin = "0.28 0.03", AnchorMax = "0.98 0.85" }
            }, PanelMain, "ContentArea");

            string contentText = "";
            switch (tab)
            {
                case "info":
                    contentText = "<b><size=18><color=#55EFC4>SERVER SPECIFICATIONS & RATES</color></size></b>\n\n" +
                                  "• <b>Gather Multiplier:</b> 2x Resources (Wood, Stone, Metal, Sulfur, Scrap)\n" +
                                  "• <b>Furnace Smelting:</b> 2x Speed for all ovens & refineries\n" +
                                  "• <b>Stack Sizes:</b> 2x Stacks for Resources, Components & Meds\n" +
                                  "• <b>Group Size Limit:</b> <color=#FAB1A0>Trio (Max 3 players per team / base)</color>\n" +
                                  "• <b>Wipe Schedule:</b> Map Wipe Every Thursday @ 14:00 CEST | Monthly Force Wipe\n\n" +
                                  "<b>Quick Chat Commands:</b>\n" +
                                  "• <color=#FFEAA7>/info</color> — Reopen this welcome screen\n" +
                                  "• <color=#FFEAA7>/shop</color> — Open in-game Store & VIP perks\n" +
                                  "• <color=#FFEAA7>/kit</color> — Claim starter and daily rewards";
                    break;

                case "rules":
                    contentText = "<b><size=18><color=#FF7675>SERVER RULES (STRICT POLICY)</color></size></b>\n\n" +
                                  "<b>1. Anti-Cheat & Fair Play:</b>\n" +
                                  "• Cheating, recoil scripts (Bloody/A4Tech software) = <b>Permanent Ban</b>.\n" +
                                  "• Playing with a banned cheater within 24h = Association Ban.\n" +
                                  "• Refusing or disconnecting during a PC Check = Permanent Ban.\n\n" +
                                  "<b>2. Trio Group Limits:</b>\n" +
                                  "• Maximum 3 players allowed to roam, raid, base-share, or team together.\n" +
                                  "• No cross-teaming, alliances, or friendly neighboring defenses.\n\n" +
                                  "<b>3. Chat Etiquette:</b>\n" +
                                  "• English in global chat. Zero tolerance for racism, doxxing, or toxicity.";
                    break;

                case "store":
                    contentText = "<b><size=18><color=#FDCB6E>VIP RANKS & EXCLUSIVE PERKS</color></size></b>\n\n" +
                                  "Get instant queue skip, kits, and skins to support the server:\n\n" +
                                  "• <b>💎 VIP (€5/mo):</b> Queue Skip, /kit vip, /skinbox access, 2x furnace boost\n" +
                                  "• <b>⭐ VIP+ (€10/mo):</b> All VIP perks, /kit vip+ (SMG/Armor), /sil sign painter, /boxsort\n" +
                                  "• <b>👑 ELITE (€15/mo):</b> Top Priority Queue, /kit elite (AK/LR/Metal), Discord Nitro role\n\n" +
                                  "🌐 <b>Visit Store:</b> <color=#74B9FF>https://worldproject.tebex.io</color>\n" +
                                  "<i>Type <color=#FFEAA7>/shop</color> in chat to open the full store browser!</i>";
                    break;

                case "discord":
                    contentText = "<b><size=18><color=#74B9FF>OFFICIAL DISCORD COMMUNITY</color></size></b>\n\n" +
                                  "Stay up to date with wipe notifications, report players, and claim giveaways:\n\n" +
                                  "🔗 <b>Invite URL:</b> <color=#55EFC4>https://discord.gg/worldrust</color>\n\n" +
                                  "• 🚨 <b>Player Reports:</b> Report suspected cheaters with video evidence\n" +
                                  "• ⏰ <b>Wipe Alerts:</b> Get 30-minute notifications before server wipes\n" +
                                  "• 🎁 <b>Giveaways:</b> Free VIP kits & skin giveaways every wipe cycle\n" +
                                  "• 🎯 <b>Team Finder:</b> Find Duo/Trio teammates in our dedicated channels";
                    break;
            }

            elements.Add(new CuiLabel
            {
                Text = { Text = contentText, FontSize = 14, Align = TextAnchor.UpperLeft, Color = "0.92 0.92 0.92 1.0" },
                RectTransform = { AnchorMin = "0.04 0.05", AnchorMax = "0.96 0.95" }
            }, "ContentArea");

            CuiHelper.AddUi(player, elements);
        }

        private void AddTabButton(CuiElementContainer elements, string parent, string title, string tabName, string yAnchor, bool active)
        {
            string[] y = yAnchor.Split(' ');
            string btnColor = active ? "0.20 0.50 0.85 1.00" : "0.14 0.16 0.22 0.90";
            string textColor = active ? "1.0 1.0 1.0 1.0" : "0.75 0.75 0.75 1.0";

            elements.Add(new CuiButton
            {
                Button = { Command = $"welcomepanel.tab {tabName}", Color = btnColor },
                RectTransform = { AnchorMin = $"0.05 {y[0]}", AnchorMax = $"0.95 {y[1]}" },
                Text = { Text = $"<b>{title}</b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = textColor }
            }, parent);
        }
    }
}
