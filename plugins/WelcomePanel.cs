using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("WelcomePanel", "WorldProject", "2.0.0")]
    [Description("Sleek Modern AAA GUI Welcome Window & Info Menu for Rust")]
    public class WelcomePanel : RustPlugin
    {
        private const string PanelMain = "WP_Welcome_Main";
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

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsConnected) return;

            timer.Once(1.2f, () =>
            {
                if (player != null && player.IsConnected)
                {
                    OpenPanel(player, "info");
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
            Effect.server.Run("assets/bundled/prefabs/fx/item_break.prefab", player.transform.position);
        }

        private void OpenPanel(BasePlayer player, string tab)
        {
            CuiHelper.DestroyUi(player, PanelMain);

            var elements = new CuiElementContainer();

            // 1. Внешний фон (Тёмный кинематографичный блюр)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.04 0.05 0.07 0.98", Material = "assets/content/ui/uibackgroundblur.mat" },
                RectTransform = { AnchorMin = "0.12 0.10", AnchorMax = "0.88 0.90" },
                CursorEnabled = true
            }, "Overlay", PanelMain);

            // 2. Верхний градиентный хедер
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.09 0.11 0.16 1.00" },
                RectTransform = { AnchorMin = "0 0.89", AnchorMax = "1 1" }
            }, PanelMain, "Header");

            // Оранжевая акцентная полоска под хедером
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.90 0.49 0.13 1.00" },
                RectTransform = { AnchorMin = "0 0.885", AnchorMax = "1 0.89" }
            }, PanelMain, "AccentLine");

            // Логотип и заголовок сервера
            elements.Add(new CuiLabel
            {
                Text = { 
                    Text = "<size=21><b><color=#E67E22>⚡ WORLD PROJECT</color></b></size>  <size=12><color=#7F8C8D>•</color></size>  <size=14><color=#ECF0F1>EU 2X TRIO</color></size>  <size=11><color=#BDC3C7>[MONTHLY WIPE]</color></size>", 
                    FontSize = 16, 
                    Align = TextAnchor.MiddleLeft, 
                    Color = "1 1 1 1" 
                },
                RectTransform = { AnchorMin = "0.03 0", AnchorMax = "0.75 1" }
            }, "Header");

            // Кнопка закрытия [✕]
            elements.Add(new CuiButton
            {
                Button = { Command = "welcomepanel.close", Color = "0.75 0.22 0.17 0.85" },
                RectTransform = { AnchorMin = "0.93 0.18", AnchorMax = "0.98 0.82" },
                Text = { Text = "<b>✕</b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "Header");

            // 3. Левая панель навигации (Sidebar)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.06 0.07 0.10 1.00" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.26 0.885" }
            }, PanelMain, "Sidebar");

            AddNavButton(elements, "Sidebar", "📌  SERVER OVERVIEW", "info", "0.78 0.94", tab == "info");
            AddNavButton(elements, "Sidebar", "📜  RULES & LIMITS", "rules", "0.60 0.76", tab == "rules");
            AddNavButton(elements, "Sidebar", "💎  VIP STORE & KITS", "store", "0.42 0.58", tab == "store");
            AddNavButton(elements, "Sidebar", "💬  DISCORD & LINKS", "discord", "0.24 0.40", tab == "discord");

            // Большая зеленая кнопка "ENTER WORLD" внизу навигации
            elements.Add(new CuiButton
            {
                Button = { Command = "welcomepanel.close", Color = "0.18 0.65 0.38 1.00" },
                RectTransform = { AnchorMin = "0.06 0.04", AnchorMax = "0.94 0.16" },
                Text = { Text = "<b>⚡ ENTER SERVER</b>", FontSize = 13, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "Sidebar");

            // 4. Основная контентная область
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.08 0.09 0.13 0.95" },
                RectTransform = { AnchorMin = "0.28 0.03", AnchorMax = "0.98 0.86" }
            }, PanelMain, "ContentArea");

            // Заполнение контентом в зависимости от выбранной вкладки
            switch (tab)
            {
                case "info":
                    RenderInfoTab(elements, "ContentArea");
                    break;
                case "rules":
                    RenderRulesTab(elements, "ContentArea");
                    break;
                case "store":
                    RenderStoreTab(elements, "ContentArea");
                    break;
                case "discord":
                    RenderDiscordTab(elements, "ContentArea");
                    break;
            }

            CuiHelper.AddUi(player, elements);
        }

        private void RenderInfoTab(CuiElementContainer elements, string parent)
        {
            // Карточка 1: Рейты
            AddFeatureCard(elements, parent, "⛏️ 2X GATHER RATES", "Wood, Stone, Metal, Sulfur & Scrap are multiplied by 2x for faster progression.", "0.03 0.66", "0.48 0.95", "0.11 0.14 0.20 0.90");
            
            // Карточка 2: Плавка
            AddFeatureCard(elements, parent, "⚡ FAST SMELTING", "2x smelting speed in all Furnaces, Large Furnaces & Refineries. Balanced charcoal.", "0.52 0.66", "0.97 0.95", "0.11 0.14 0.20 0.90");

            // Карточка 3: Стаки
            AddFeatureCard(elements, parent, "📦 2X STACK SIZES", "Increased stacks for raw resources, ammo, and medical supplies to save chest space.", "0.03 0.33", "0.48 0.62", "0.11 0.14 0.20 0.90");

            // Карточка 4: Лимиты
            AddFeatureCard(elements, parent, "👥 TRIO LIMIT (MAX 3)", "Strict maximum of 3 players per team, roam, or base. No teaming / alliances.", "0.52 0.33", "0.97 0.62", "0.11 0.14 0.20 0.90");

            // Нижняя плашка с расписанием вайпов и командами
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.12 0.15 0.22 0.95" },
                RectTransform = { AnchorMin = "0.03 0.03", AnchorMax = "0.97 0.29" }
            }, parent, "BottomInfo");

            elements.Add(new CuiLabel
            {
                Text = { 
                    Text = "<b>⏰ WIPE SCHEDULE:</b> Every Thursday @ 14:00 CEST (BP Kept)  |  Force Wipe: 1st Thursday @ 20:00 CEST\n" +
                           "<b>🎮 QUICK COMMANDS:</b> <color=#F1C40F>/info</color> — Menu  |  <color=#F1C40F>/shop</color> — Store  |  <color=#F1C40F>/kit</color> — Free Kits  |  <color=#F1C40F>/mydonate</color> — VIP Status", 
                    FontSize = 12, 
                    Align = TextAnchor.MiddleLeft, 
                    Color = "0.92 0.92 0.92 1.0" 
                },
                RectTransform = { AnchorMin = "0.04 0", AnchorMax = "0.96 1" }
            }, "BottomInfo");
        }

        private void RenderRulesTab(CuiElementContainer elements, string parent)
        {
            AddRuleCard(elements, parent, "🚫 1. ANTI-CHEAT & RECOIL SCRIPTS", 
                "• Strict ban for cheats, scripts, macro software (Bloody / A4Tech).\n• Refusing or failing a staff PC-Check (AnyDesk) = Permanent Ban.", 
                "0.03 0.68", "0.97 0.95", "0.75 0.22 0.17 0.25", "0.90 0.30 0.23 1.0");

            AddRuleCard(elements, parent, "👥 2. GROUP LIMITS (TRIO MAX 3)", 
                "• Maximum 3 players in a base, roaming, raiding, or defending together.\n• No cross-teaming, alliances, or friendly neighbors defending each other.", 
                "0.03 0.36", "0.97 0.64", "0.90 0.49 0.13 0.25", "0.90 0.49 0.13 1.0");

            AddRuleCard(elements, parent, "💬 3. CHAT ETIQUETTE & FAIR PLAY", 
                "• English only in public global chat. Zero tolerance for racism, doxxing, or extreme toxicity.", 
                "0.03 0.04", "0.97 0.32", "0.20 0.50 0.85 0.25", "0.20 0.50 0.85 1.0");
        }

        private void RenderStoreTab(CuiElementContainer elements, string parent)
        {
            AddStoreCard(elements, parent, "💎 VIP TIER", "€5 / mo", 
                "• Queue Skip (No Waiting)\n• /kit vip (SAR, Hazmat, Meds)\n• Access to /skinbox\n• 2x Smelt Bonus\n• [VIP] Chat Tag", 
                "0.03 0.05", "0.32 0.95", "0.15 0.35 0.55 0.85", "0.20 0.60 0.90 1.0");

            AddStoreCard(elements, parent, "⭐ VIP+ TIER", "€10 / mo", 
                "• All VIP Perks Included\n• /kit vip+ (SMG, Roadsign)\n• /sil (Paint signs from URL)\n• /boxsort (Auto Sort Chests)\n• [VIP+] Gold Tag", 
                "0.35 0.05", "0.65 0.95", "0.55 0.35 0.15 0.85", "0.95 0.60 0.10 1.0");

            AddStoreCard(elements, parent, "👑 ELITE TIER", "€15 / mo", 
                "• Ultimate Priority Queue\n• /kit elite (AK-47 / Full Metal)\n• All VIP & VIP+ Features\n• Custom Discord Nitro Role\n• Priority Support", 
                "0.68 0.05", "0.97 0.95", "0.40 0.15 0.50 0.85", "0.75 0.25 0.85 1.0");
        }

        private void RenderDiscordTab(CuiElementContainer elements, string parent)
        {
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.11 0.14 0.20 0.90" },
                RectTransform = { AnchorMin = "0.05 0.05", AnchorMax = "0.95 0.95" }
            }, parent, "DiscordCard");

            elements.Add(new CuiLabel
            {
                Text = { 
                    Text = "<b><size=22><color=#74B9FF>JOIN THE WORLD PROJECT DISCORD</color></size></b>\n\n" +
                           "Stay connected with the community, get wipe alerts, report cheaters and claim giveaways:\n\n" +
                           "🌐 <b><size=20><color=#55EFC4>https://discord.gg/worldrust</color></size></b>\n\n" +
                           "• 🚨 <b>Player Reports:</b> Report suspected cheaters with instant staff review\n" +
                           "• ⏰ <b>Wipe Notifications:</b> 30-minute reminder before wipes\n" +
                           "• 🎁 <b>Weekly Giveaways:</b> Free VIP kits & skin giveaways\n" +
                           "• 🎯 <b>Team Finder:</b> Find Duo/Trio teammates easily", 
                    FontSize = 14, 
                    Align = TextAnchor.MiddleCenter, 
                    Color = "0.95 0.95 0.95 1.0" 
                },
                RectTransform = { AnchorMin = "0.05 0.05", AnchorMax = "0.95 0.95" }
            }, "DiscordCard");
        }

        private void AddNavButton(CuiElementContainer elements, string parent, string title, string tabName, string yAnchor, bool active)
        {
            string[] y = yAnchor.Split(' ');
            string btnColor = active ? "0.14 0.18 0.26 1.00" : "0.08 0.09 0.13 0.80";
            string textColor = active ? "1.0 1.0 1.0 1.0" : "0.65 0.65 0.65 1.0";

            string btnId = CuiHelper.GetGuid();

            elements.Add(new CuiButton
            {
                Button = { Command = $"welcomepanel.tab {tabName}", Color = btnColor },
                RectTransform = { AnchorMin = $"0.05 {y[0]}", AnchorMax = $"0.95 {y[1]}" },
                Text = { Text = $"<b>{title}</b>", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = textColor }
            }, parent, btnId);

            // Если кнопка активна — рисуем слева яркую оранжевую полоску
            if (active)
            {
                elements.Add(new CuiPanel
                {
                    Image = { Color = "0.90 0.49 0.13 1.00" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0.04 1" }
                }, btnId);
            }
        }

        private void AddFeatureCard(CuiElementContainer elements, string parent, string title, string desc, string min, string max, string bgColor)
        {
            string cardId = CuiHelper.GetGuid();
            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><color=#E67E22>{title}</color></b>", FontSize = 14, Align = TextAnchor.MiddleLeft, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0.06 0.65", AnchorMax = "0.94 0.92" }
            }, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = desc, FontSize = 11, Align = TextAnchor.UpperLeft, Color = "0.80 0.80 0.80 1.0" },
                RectTransform = { AnchorMin = "0.06 0.08", AnchorMax = "0.94 0.62" }
            }, cardId);
        }

        private void AddRuleCard(CuiElementContainer elements, string parent, string title, string desc, string min, string max, string bgColor, string titleColor)
        {
            string cardId = CuiHelper.GetGuid();
            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b>{title}</b>", FontSize = 14, Align = TextAnchor.MiddleLeft, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0.04 0.62", AnchorMax = "0.96 0.92" }
            }, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = desc, FontSize = 11, Align = TextAnchor.UpperLeft, Color = "0.85 0.85 0.85 1.0" },
                RectTransform = { AnchorMin = "0.04 0.08", AnchorMax = "0.96 0.60" }
            }, cardId);
        }

        private void AddStoreCard(CuiElementContainer elements, string parent, string title, string price, string desc, string min, string max, string bgColor, string accentColor)
        {
            string cardId = CuiHelper.GetGuid();
            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            // Верхняя плашка названия
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b>{title}</b>", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 0.98" }
            }, cardId);

            // Цена
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><color=#F1C40F>{price}</color></b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.74", AnchorMax = "1 0.85" }
            }, cardId);

            // Список перков
            elements.Add(new CuiLabel
            {
                Text = { Text = desc, FontSize = 11, Align = TextAnchor.UpperLeft, Color = "0.90 0.90 0.90 1.0" },
                RectTransform = { AnchorMin = "0.08 0.18", AnchorMax = "0.92 0.72" }
            }, cardId);

            // Кнопка перехода
            elements.Add(new CuiButton
            {
                Button = { Command = "welcomepanel.tab store", Color = "0.18 0.65 0.38 0.95" },
                RectTransform = { AnchorMin = "0.08 0.04", AnchorMax = "0.92 0.15" },
                Text = { Text = "<b>VIEW STORE</b>", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, cardId);
        }
    }
}
