using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUIShop", "WorldProject", "1.0.0")]
    [Description("In-game graphical store and VIP package browser")]
    public class GUIShop : RustPlugin
    {
        private const string ShopPanel = "GUIShop_Main";

        private void Init()
        {
            cmd.AddChatCommand("shop", this, nameof(CmdOpenShop));
            cmd.AddChatCommand("store", this, nameof(CmdOpenShop));
            cmd.AddChatCommand("buy", this, nameof(CmdOpenShop));
        }

        private void CmdOpenShop(BasePlayer player, string command, string[] args)
        {
            string category = args.Length > 0 ? args[0].ToLower() : "vip";
            OpenShopUI(player, category);
        }

        [ConsoleCommand("guishop.category")]
        private void ConsoleCategory(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            string category = arg.GetString(0, "vip");
            OpenShopUI(player, category);
        }

        [ConsoleCommand("guishop.close")]
        private void ConsoleClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, ShopPanel);
        }

        private void OpenShopUI(BasePlayer player, string category)
        {
            CuiHelper.DestroyUi(player, ShopPanel);

            var elements = new CuiElementContainer();

            // Фон окна магазина
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.07 0.08 0.10 0.96" },
                RectTransform = { AnchorMin = "0.12 0.12", AnchorMax = "0.88 0.88" },
                CursorEnabled = true
            }, "Overlay", ShopPanel);

            // Шапка магазина
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.95 0.60 0.07 1.00" },
                RectTransform = { AnchorMin = "0 0.90", AnchorMax = "1 1" }
            }, ShopPanel, "ShopHeader");

            elements.Add(new CuiLabel
            {
                Text = { Text = "<b>🛒 WORLD PROJECT | IN-GAME STORE & VIP</b>", FontSize = 18, Align = TextAnchor.MiddleLeft, Color = "0.05 0.05 0.05 1.0" },
                RectTransform = { AnchorMin = "0.03 0", AnchorMax = "0.8 1" }
            }, "ShopHeader");

            // Кнопка закрытия [✕]
            elements.Add(new CuiButton
            {
                Button = { Command = "guishop.close", Color = "0.8 0.15 0.15 1.0" },
                RectTransform = { AnchorMin = "0.92 0.15", AnchorMax = "0.98 0.85" },
                Text = { Text = "<b>✕</b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "ShopHeader");

            // Категории (Верхнее меню)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.12 0.14 0.18 1.00" },
                RectTransform = { AnchorMin = "0 0.81", AnchorMax = "1 0.90" }
            }, ShopPanel, "CategoryBar");

            AddCategoryTab(elements, "CategoryBar", "💎 VIP RANKS", "vip", "0.02 0.23", category == "vip");
            AddCategoryTab(elements, "CategoryBar", "📦 STARTER KITS", "kits", "0.25 0.46", category == "kits");
            AddCategoryTab(elements, "CategoryBar", "🌲 RESOURCES", "resources", "0.48 0.69", category == "resources");
            AddCategoryTab(elements, "CategoryBar", "🌐 WEBSTORE LINK", "web", "0.71 0.98", category == "web");

            // Контентная зона магазина
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.10 0.12 0.15 0.60" },
                RectTransform = { AnchorMin = "0.02 0.03", AnchorMax = "0.98 0.79" }
            }, ShopPanel, "ShopContent");

            if (category == "vip")
            {
                // Карточка 1: VIP
                AddShopCard(elements, "ShopContent", "💎 VIP TIER", "€5.00 / month", 
                    "• Queue Skip (Skip waiting)\n• /kit vip every 4 hours\n• Access to /skinbox\n• 2x Furnace smelting bonus\n• [VIP] Chat & Discord tag", 
                    "0.02 0.05", "0.32 0.95", "0.18 0.35 0.55 0.85");

                // Карточка 2: VIP+
                AddShopCard(elements, "ShopContent", "⭐ VIP+ TIER", "€10.00 / month", 
                    "• All VIP Perks Included\n• /kit vip+ (SMG & Roadsign)\n• Access to /sil (Sign Painter)\n• Access to /boxsort (Chest Sort)\n• [VIP+] Gold Chat tag", 
                    "0.35 0.05", "0.65 0.95", "0.55 0.35 0.18 0.85");

                // Карточка 3: ELITE
                AddShopCard(elements, "ShopContent", "👑 ELITE TIER", "€15.00 / month", 
                    "• Instant Priority Queue\n• /kit elite (AK-47 & Metal)\n• All VIP & VIP+ Features\n• Custom Discord Nitro Role\n• Priority Ticket Support", 
                    "0.68 0.05", "0.98 0.95", "0.45 0.15 0.55 0.85");
            }
            else if (category == "kits")
            {
                AddShopCard(elements, "ShopContent", "📦 STARTER KIT", "FREE (Cooldown: 1h)", 
                    "• Stone Hatchet & Pickaxe\n• Hunting Bow + 30 Arrows\n• 2x Bandages\n• 1x Campfire\n\nClaim in chat: /kit starter", 
                    "0.05 0.15", "0.47 0.85", "0.15 0.35 0.25 0.85");

                AddShopCard(elements, "ShopContent", "🎁 DAILY REWARD", "FREE (Cooldown: 24h)", 
                    "• 1,000 Wood & 1,000 Stone\n• 100 Low Grade Fuel\n• 2x Medical Syringes\n• 50 Scrap\n\nClaim in chat: /kit daily", 
                    "0.53 0.15", "0.95 0.85", "0.25 0.25 0.45 0.85");
            }
            else
            {
                elements.Add(new CuiLabel
                {
                    Text = { 
                        Text = "<b><size=20><color=#F1C40F>OFFICIAL SERVER WEBSTORE</color></size></b>\n\n" +
                               "Visit our online store to browse all available packages, kits, and perks:\n\n" +
                               "🌐 <b><size=22><color=#74B9FF>https://worldproject.tebex.io</color></size></b>\n\n" +
                               "• Automatic Instant Delivery (within 60s)\n" +
                               "• Secure payment via PayPal, Credit Cards, Apple Pay, Google Pay\n" +
                               "• Direct synchronization with Discord and in-game permissions!",
                        FontSize = 15,
                        Align = TextAnchor.MiddleCenter,
                        Color = "0.95 0.95 0.95 1.0"
                    },
                    RectTransform = { AnchorMin = "0.05 0.05", AnchorMax = "0.95 0.95" }
                }, "ShopContent");
            }

            CuiHelper.AddUi(player, elements);
        }

        private void AddCategoryTab(CuiElementContainer elements, string parent, string title, string catName, string xAnchor, bool active)
        {
            string[] x = xAnchor.Split(' ');
            string btnColor = active ? "0.95 0.60 0.07 1.00" : "0.18 0.20 0.26 0.90";
            string textColor = active ? "0.05 0.05 0.05 1.0" : "0.85 0.85 0.85 1.0";

            elements.Add(new CuiButton
            {
                Button = { Command = $"guishop.category {catName}", Color = btnColor },
                RectTransform = { AnchorMin = $"{x[0]} 0.12", AnchorMax = $"{x[1]} 0.88" },
                Text = { Text = $"<b>{title}</b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = textColor }
            }, parent);
        }

        private void AddShopCard(CuiElementContainer elements, string parent, string title, string price, string description, string min, string max, string bgColor)
        {
            string cardId = CuiHelper.GetGuid();

            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            // Название карточки
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b>{title}</b>", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.84", AnchorMax = "1 0.98" }
            }, cardId);

            // Цена
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><color=#F1C40F>{price}</color></b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.72", AnchorMax = "1 0.84" }
            }, cardId);

            // Описание
            elements.Add(new CuiLabel
            {
                Text = { Text = description, FontSize = 12, Align = TextAnchor.UpperLeft, Color = "0.9 0.9 0.9 1.0" },
                RectTransform = { AnchorMin = "0.08 0.18", AnchorMax = "0.92 0.70" }
            }, cardId);

            // Кнопка ссылки
            elements.Add(new CuiButton
            {
                Button = { Command = "guishop.category web", Color = "0.15 0.75 0.35 0.95" },
                RectTransform = { AnchorMin = "0.08 0.04", AnchorMax = "0.92 0.15" },
                Text = { Text = "<b>VIEW ON STORE</b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, cardId);
        }
    }
}
